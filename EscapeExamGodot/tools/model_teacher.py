# -*- coding: utf-8 -*-
"""
教师 V3 模型生成脚本（Blender 4.2，无头运行）。

用法：
  blender --background --python tools/model_teacher.py -- <输出目录>
产出：<输出目录>/teacher_model.glb

设计口径（见 docs/v2/V2_DESIGN_BIBLE.md「终场秩序」+ 第五人格式剪影标准）：
- 几何全部按"抽象坐标"构建：u=右，v=上，w=正面方向（朝向角色前方）。
  抽象点 (u, v, w) → Blender (u, -w, v)；角色在 Blender 中面朝 -Y，
  glTF 导出后在 Godot 中面朝 -Z（与 teacher.gd 的本地前方一致）。
- 节点层级与现行 teacher.tscn 的 $Visual 契约完全一致：
  Visual/{Coat, CoatFlare, CoatSkirt, LapelL/R, Collar, ShirtFront, Tie,
          Placket, LeftLeg/{Trousers, Shoe}, RightLeg/{...},
          LeftArm/{Sleeve, Hand, PaperRoll}, RightArm/{Sleeve, Hand, RedPen, PaperFolder},
          KeyChain/{Ring, Key1..3}, Head/{Face, Neck, Hair, HairSideL/R, EarL/R,
          Nose, EyeL/R, BrowL/R, GlassL/R, GlassesBridge, Smile, SmileCornerL/R}}
  teacher.gd 只旋转 LeftLeg/RightLeg/LeftArm/RightArm/Head 五个枢轴组，枢轴位置
  与现行 tscn 一致（髋 y=1.05、肩 x=±0.34 y=1.80、头 y=2.02，Godot 坐标）。
- 材质在 Blender 内以 Principled 参数给出（与 V2 调色一致），glTF 携带。
"""
import bpy
import math
import os
import sys
from mathutils import Vector

# ---------------------------------------------------------------- 基础工具

def B(u, v, w):
    """抽象坐标 → Blender 坐标。角色在 Blender 中面朝 +Y，
    glTF 导出后在 Godot 中面朝 -Z（实测标定，见 QA 记录）。
    该映射行列式为 -1（镜像），故 make_obj 统一翻转面绕序。"""
    return (u, w, v)

def clean_scene():
    bpy.ops.object.select_all(action="SELECT")
    bpy.ops.object.delete(use_global=False)
    for coll in (bpy.data.meshes, bpy.data.curves, bpy.data.materials, bpy.data.cameras, bpy.data.lights):
        pass  # 材质按名复用/新建，不强制清理数据块

MATS = {}
def mat(name, color, roughness=0.85, metallic=0.0):
    if name in MATS:
        return MATS[name]
    m = bpy.data.materials.new(name)
    m.use_nodes = True
    bsdf = m.node_tree.nodes["Principled BSDF"]
    bsdf.inputs["Base Color"].default_value = (color[0], color[1], color[2], 1.0)
    bsdf.inputs["Roughness"].default_value = roughness
    bsdf.inputs["Metallic"].default_value = metallic
    MATS[name] = m
    return m

def make_obj(name, verts, faces, material, smooth=True):
    """verts/faces 均用抽象坐标。"""
    bv = [B(*v) for v in verts]
    faces = [tuple(reversed(f)) for f in faces]  # 抵消 B 的镜像
    me = bpy.data.meshes.new(name)
    me.from_pydata(bv, [], faces)
    me.update()
    ob = bpy.data.objects.new(name, me)
    bpy.context.collection.objects.link(ob)
    if material is not None:
        me.materials.append(material)
    if smooth:
        for p in me.polygons:
            p.use_smooth = True
    return ob

def lathe(name, profile, material, seg=28, cap_bottom=True, cap_top=True):
    """绕 v 轴的椭圆截面旋转体。profile: [(v, ru, rw, w_off), ...] 自下而上。"""
    verts, faces = [], []
    rings = []
    for (v, ru, rw, wo) in profile:
        ring = []
        for i in range(seg):
            a = 2.0 * math.pi * i / seg
            ring.append(len(verts))
            verts.append((ru * math.cos(a), v, rw * math.sin(a) + wo))
        rings.append(ring)
    for r in range(len(rings) - 1):
        for i in range(seg):
            j = (i + 1) % seg
            faces.append((rings[r][i], rings[r][j], rings[r + 1][j], rings[r + 1][i]))
    if cap_bottom:
        v0, ru0, rw0, wo0 = profile[0]
        c = len(verts); verts.append((0.0, v0 - 0.004, wo0))
        for i in range(seg):
            j = (i + 1) % seg
            faces.append((c, rings[0][j], rings[0][i]))
    if cap_top:
        v1, ru1, rw1, wo1 = profile[-1]
        c = len(verts); verts.append((0.0, v1 + 0.004, wo1))
        for i in range(seg):
            j = (i + 1) % seg
            faces.append((c, rings[-1][i], rings[-1][j]))
    return make_obj(name, verts, faces, material)

def tube(name, points, radius, material, seg=8):
    """沿折线（抽象坐标点列）的圆管。"""
    verts, faces = [], []
    pts = [Vector(p) for p in points]
    rings = []
    for idx, p in enumerate(pts):
        if idx == 0:
            t = (pts[1] - pts[0]).normalized()
        elif idx == len(pts) - 1:
            t = (pts[-1] - pts[-2]).normalized()
        else:
            t = (pts[idx + 1] - pts[idx - 1]).normalized()
        ref = Vector((1.0, 0.0, 0.0))
        if abs(t.dot(ref)) > 0.9:
            ref = Vector((0.0, 0.0, 1.0))
        b1 = ref - t * ref.dot(t); b1.normalize()
        b2 = t.cross(b1)
        ring = []
        for i in range(seg):
            a = 2.0 * math.pi * i / seg
            off = b1 * (radius * math.cos(a)) + b2 * (radius * math.sin(a))
            q = p + off
            ring.append(len(verts))
            verts.append((q.x, q.y, q.z))
        rings.append(ring)
    for r in range(len(rings) - 1):
        for i in range(seg):
            j = (i + 1) % seg
            faces.append((rings[r][i], rings[r][j], rings[r + 1][j], rings[r + 1][i]))
    for end, ring in ((0, rings[0]), (1, rings[-1])):
        c = len(verts)
        verts.append(tuple(pts[0] if end == 0 else pts[-1]))
        for i in range(seg):
            j = (i + 1) % seg
            if end == 0:
                faces.append((c, ring[j], ring[i]))
            else:
                faces.append((c, ring[i], ring[j]))
    return make_obj(name, verts, faces, material)

def rot_u(p, ang, center=(0.0, 0.0, 0.0)):
    """绕 u 轴（v-w 平面）旋转抽象点。"""
    u, v, w = p[0] - center[0], p[1] - center[1], p[2] - center[2]
    ca, sa = math.cos(ang), math.sin(ang)
    return (p[0], center[1] + v * ca - w * sa, center[2] + v * sa + w * ca)

def rot_w(p, ang, center=(0.0, 0.0, 0.0)):
    """绕 w 轴（u-v 平面）旋转抽象点。"""
    u, v, w = p[0] - center[0], p[1] - center[1], p[2] - center[2]
    ca, sa = math.cos(ang), math.sin(ang)
    return (center[0] + u * ca - v * sa, center[1] + u * sa + v * ca, p[2])

def beveled_box(name, size, center, material, bevel=0.012, taper=None, rot=None):
    """圆角方盒；taper=(top_scale, bottom_scale) 按 v 缩放 u/w；rot=('u'|'w', ang)。"""
    sx, sy, sz = size[0] / 2, size[1] / 2, size[2] / 2
    import bmesh
    me = bpy.data.meshes.new(name)
    bm = bmesh.new()
    bmesh.ops.create_cube(bm, size=2.0)
    for vert in bm.verts:
        x, y, z = vert.co.x, vert.co.y, vert.co.z  # Blender: x=u, y=-w, z=v
        v = z
        if taper is not None:
            s = taper[1] + (taper[0] - taper[1]) * (v + 1.0) / 2.0
            x *= s; y *= s
        vert.co = Vector((x * sx, y * sz, z * sy))
    bm.to_mesh(me)
    bm.free()
    ob = bpy.data.objects.new(name, me)
    bpy.context.collection.objects.link(ob)
    if material is not None:
        me.materials.append(material)
    bpy.context.view_layer.objects.active = ob
    ob.select_set(True)
    bev = ob.modifiers.new("bev", "BEVEL")
    bev.width = bevel
    bev.segments = 3
    bpy.ops.object.modifier_apply(modifier="bev")
    ob.select_set(False)
    # 把几何搬到抽象 center（Blender 坐标换算）
    cx, cy, cz = B(*center)
    for vert in me.vertices:
        vert.co.x += cx; vert.co.y += cy; vert.co.z += cz
    if rot is not None:
        axis, ang = rot
        cc = B(*center)
        for vert in me.vertices:
            p = (vert.co.x - 0.0, 0.0, 0.0)  # 占位，下面按抽象坐标旋转
        # 用抽象坐标旋转再写回
        for vert in me.vertices:
            a = (vert.co.x, vert.co.z, vert.co.y)  # Blender → 抽象 (u, v, w)
            if axis == 'u':
                a = rot_u(a, ang, center)
            else:
                a = rot_w(a, ang, center)
            bx, by, bz = B(*a)
            vert.co = Vector((bx, by, bz))
    for p in me.polygons:
        p.use_smooth = False
    me.update()
    return ob

def flat_plate(name, outline_uv, w_center, thickness, material, rot_u_ang=0.0, v_center=0.0):
    """u-v 平面内的 2D 轮廓挤出成薄板（w 方向厚度），可绕 u 轴倾斜。"""
    verts, faces = [], []
    n = len(outline_uv)
    for (u, v) in outline_uv:
        verts.append((u, v, w_center - thickness / 2))
    for (u, v) in outline_uv:
        verts.append((u, v, w_center + thickness / 2))
    faces.append(tuple(range(n - 1, -1, -1)))
    faces.append(tuple(range(n, 2 * n)))
    for i in range(n):
        j = (i + 1) % n
        faces.append((i, j, n + j, n + i))
    if rot_u_ang != 0.0:
        verts = [rot_u(p, rot_u_ang, (0.0, v_center, w_center)) for p in verts]
    return make_obj(name, verts, faces, material, smooth=False)

def sphere(name, center, radii, material, shape_fn=None, seg=36, rings=24):
    """椭球；shape_fn(u,v,w 相对中心) -> 修正后相对坐标。"""
    verts, faces = [], []
    cu, cv, cw = center
    ru, rv, rw = radii
    poles = []
    for r in range(rings + 1):
        phi = math.pi * r / rings  # 0=顶
        ring = []
        for s in range(seg):
            th = 2.0 * math.pi * s / seg
            du = ru * math.sin(phi) * math.cos(th)
            dv = rv * math.cos(phi)
            dw = rw * math.sin(phi) * math.sin(th)
            if shape_fn is not None:
                du, dv, dw = shape_fn(du, dv, dw)
            ring.append(len(verts))
            verts.append((cu + du, cv + dv, cw + dw))
        poles.append(ring)
    for r in range(rings):
        for s in range(seg):
            j = (s + 1) % seg
            faces.append((poles[r][s], poles[r][j], poles[r + 1][j], poles[r + 1][s]))
    return make_obj(name, verts, faces, material)

def merge_objs(name, objs, material=None):
    """把多个对象合并为一个（保留各自几何的世界/本地抽象坐标不变，调用前均未设父级变换）。"""
    bpy.ops.object.select_all(action="DESELECT")
    for o in objs:
        o.select_set(True)
    bpy.context.view_layer.objects.active = objs[0]
    bpy.ops.object.join()
    merged = bpy.context.view_layer.objects.active
    merged.name = name
    if material is not None:
        merged.data.materials.clear()
        merged.data.materials.append(material)
    return merged

# ---------------------------------------------------------------- 材质
COAT   = mat("CoatMain",  (0.180, 0.200, 0.212), 0.86)
COATD  = mat("CoatDark",  (0.137, 0.153, 0.161), 0.88)
SKIN   = mat("SkinPale",  (0.796, 0.725, 0.651), 0.62)
EYED   = mat("EyeDark",   (0.045, 0.045, 0.050), 0.22, 0.10)
TRIM   = mat("DarkTrim",  (0.070, 0.070, 0.070), 0.50, 0.20)
RED    = mat("SealRed",   (0.616, 0.314, 0.282), 0.42)
PAPER  = mat("ExamPaper", (0.898, 0.847, 0.722), 0.82)
KEYM   = mat("KeyMetal",  (0.720, 0.620, 0.380), 0.28, 0.90)
TROUS  = mat("Trousers",  (0.110, 0.125, 0.140), 0.90)
SHOE   = mat("HardShoe",  (0.050, 0.050, 0.055), 0.38, 0.05)
HAIR   = mat("HairGrey",  (0.145, 0.135, 0.125), 0.78)
GLASSM = mat("GlassMetal",(0.100, 0.100, 0.110), 0.35, 0.80)
TIE    = mat("TieDark",   (0.085, 0.095, 0.125), 0.70)

# ---------------------------------------------------------------- 层级
def empty(name, pos_abstract):
    ob = bpy.data.objects.new(name, None)
    bpy.context.collection.objects.link(ob)
    ob.location = B(*pos_abstract)
    ob.empty_display_size = 0.05
    return ob

def parent_to(child, parent):
    bpy.context.view_layer.update()
    child.parent = parent
    child.matrix_parent_inverse = parent.matrix_world.inverted()

def place(ob, parent, pos_abstract):
    """把对象放到父级局部的抽象位置（几何已在局部坐标内构建）。"""
    pw = Vector(B(*pos_abstract))
    # 父级世界平移 = 祖先链各级 location 之和（全程无旋转/缩放）
    world = Vector((0.0, 0.0, 0.0))
    p = parent
    while p is not None:
        world += p.location
        p = p.parent
    ob.location = pw + world
    if parent is not None:
        parent_to(ob, parent)

# 实际上各级 empty 的 location 已是其父级局部坐标；对象世界位置 = 祖先链 location 之和 + 自身 location。
# place() 中 ob.location 应设为“父级局部”值 pw，Blender 会按层级自动合成世界变换，
# 但 parent_to 用 matrix_parent_inverse 固定世界变换，故须把 ob.location 设为世界值后再绑定。

def build():
    clean_scene()
    visual = None  # 不建 Visual 根：Godot 导入 glTF 时以文件名生成根节点，tscn 中将其命名为 Visual

    # ---------------- 躯干（Visual 局部 = 角色坐标，v 为离地高度） ----------------
    coat = lathe("Coat", [
        (0.970, 0.218, 0.114, 0.0), (1.020, 0.205, 0.108, 0.0),
        (1.050, 0.196, 0.103, 0.0), (1.220, 0.169, 0.091, 0.0),
        (1.380, 0.178, 0.096, 0.0), (1.550, 0.205, 0.107, 0.0),
        (1.720, 0.224, 0.113, 0.0), (1.860, 0.236, 0.118, 0.0),
        (1.940, 0.222, 0.110, 0.0), (1.990, 0.150, 0.082, 0.0),
        (2.010, 0.105, 0.066, 0.0)], COAT)
    place(coat, visual, (0.0, 0.0, 0.0))

    flare = lathe("CoatFlare", [
        (0.920, 0.232, 0.120, 0.0), (0.990, 0.214, 0.110, 0.0),
        (1.050, 0.200, 0.104, 0.0)], COATD, cap_bottom=False, cap_top=False)
    place(flare, visual, (0.0, 0.0, 0.0))

    skirt = lathe("CoatSkirt", [
        (0.300, 0.318, 0.164, 0.0), (0.340, 0.300, 0.156, 0.0),
        (0.550, 0.272, 0.141, 0.0), (0.800, 0.238, 0.124, 0.0),
        (0.940, 0.205, 0.106, 0.0), (1.030, 0.185, 0.094, 0.0)], COATD)
    place(skirt, visual, (0.0, 0.0, 0.0))

    collar = lathe("Collar", [
        (1.950, 0.104, 0.074, 0.0), (2.005, 0.110, 0.078, 0.0),
        (2.048, 0.119, 0.085, 0.0)], COATD, cap_bottom=False, cap_top=False)
    place(collar, visual, (0.0, 0.0, 0.0))

    shirt = flat_plate("ShirtFront", [(-0.052, 1.700), (0.052, 1.700), (0.0, 1.975)],
                       0.100, 0.006, PAPER, rot_u_ang=-0.07, v_center=1.84)
    place(shirt, visual, (0.0, 0.0, 0.0))

    tie = flat_plate("Tie", [(-0.026, 1.975), (0.026, 1.975), (0.016, 1.700),
                             (0.0, 1.635), (-0.016, 1.700)],
                     0.106, 0.005, TIE, rot_u_ang=-0.07, v_center=1.84)
    place(tie, visual, (0.0, 0.0, 0.0))

    lapel_r = flat_plate("LapelR", [(0.012, 1.965), (0.088, 1.905), (0.120, 1.665),
                                    (0.056, 1.600), (0.016, 1.780)],
                         0.109, 0.007, COAT, rot_u_ang=-0.09, v_center=1.78)
    place(lapel_r, visual, (0.0, 0.0, 0.0))
    lapel_l = flat_plate("LapelL", [(-0.012, 1.965), (-0.088, 1.905), (-0.120, 1.665),
                                    (-0.056, 1.600), (-0.016, 1.780)],
                         0.109, 0.007, COAT, rot_u_ang=-0.09, v_center=1.78)
    place(lapel_l, visual, (0.0, 0.0, 0.0))

    # 纽扣（无门襟盒：V1 的门襟盒在正面投出一条假拉链影，QA 渲染后去除）
    placket_parts = []
    for i, (bv, bw) in enumerate([(1.78, 0.1155), (1.57, 0.1085), (1.36, 0.1005), (1.15, 0.0955)]):
        placket_parts.append(sphere("tmp_btn%d" % i, (0.0, bv, bw), (0.0105, 0.0105, 0.005), COATD, seg=14, rings=10))
    placket = merge_objs("Placket", placket_parts, COATD)
    place(placket, visual, (0.0, 0.0, 0.0))

    # ---------------- 腿（枢轴：髋 v=1.05，u=±0.10） ----------------
    def build_leg(side_u, group_name):
        g = empty(group_name, (side_u, 1.05, 0.0))
        parent_to(g, visual) if visual is not None else None
        trousers = lathe("Trousers", [
            (-0.950, 0.052, 0.056, 0.0), (-0.700, 0.058, 0.062, 0.0),
            (-0.450, 0.068, 0.072, 0.0), (-0.100, 0.076, 0.080, 0.0)], TROUS)
        # 注：裤腿顶端止于髋下（世界 v≈0.95，裙摆与腰带环覆盖区内）。
        # 曾把裤腿伸到 v≈1.15 的收腰段：两椭圆截面偏心相交，裤腿在
        # u≈±0.155 处穿出大衣正面约 6mm（V3 渲染实测的“口袋状”穿模），
        # 只缩顶环半径不够，必须降顶。
        place(trousers, g, (0.0, 0.0, 0.0))
        shoe = beveled_box("Shoe", (0.148, 0.092, 0.300), (0.0, -1.020, 0.050),
                           SHOE, bevel=0.026, taper=(0.86, 1.0))
        place(shoe, g, (0.0, 0.0, 0.0))
        return g
    build_leg(-0.10, "LeftLeg")
    build_leg(0.10, "RightLeg")

    # ---------------- 臂（枢轴：肩 v=1.80，u=±0.34） ----------------
    def build_arm(side_u, group_name, is_right):
        g = empty(group_name, (side_u, 1.80, 0.0))
        parent_to(g, visual) if visual is not None else None
        sleeve = lathe("Sleeve", [
            (-0.880, 0.058, 0.055, 0.058), (-0.820, 0.051, 0.049, 0.052),
            (-0.620, 0.056, 0.054, 0.034), (-0.420, 0.064, 0.062, 0.016),
            (-0.200, 0.074, 0.072, 0.004), (0.000, 0.086, 0.084, 0.0),
            (0.045, 0.062, 0.062, 0.0)], COAT)
        place(sleeve, g, (0.0, 0.0, 0.0))
        # 手：掌 + 指 + 拇指合并
        sgn = 1.0 if is_right else -1.0
        parts = []
        parts.append(beveled_box("tmp_palm", (0.072, 0.125, 0.034), (0.0, -0.955, 0.062),
                                 SKIN, bevel=0.013, taper=(1.0, 0.88)))
        fing = beveled_box("tmp_fing", (0.064, 0.078, 0.030), (0.0, -1.045, 0.066),
                           SKIN, bevel=0.012, taper=(0.82, 1.0))
        parts.append(fing)
        parts.append(beveled_box("tmp_thumb", (0.024, 0.068, 0.026), (-sgn * 0.043, -0.955, 0.070),
                                 SKIN, bevel=0.010, rot=('w', sgn * 0.45)))
        hand = merge_objs("Hand", parts, SKIN)
        place(hand, g, (0.0, 0.0, 0.0))
        return g

    left_arm = build_arm(-0.325, "LeftArm", False)
    right_arm = build_arm(0.325, "RightArm", True)

    # 左臂试卷筒
    roll_dir = Vector((0.12, -0.18, 0.98)).normalized()
    roll_c = Vector((0.06, -0.58, 0.16))
    p1 = roll_c - roll_dir * 0.19
    p2 = roll_c + roll_dir * 0.19
    roll = tube("PaperRoll", [tuple(p1), tuple(p2)], 0.055, PAPER, seg=18)
    # 卷边圆环（前端）
    ring_pts = []
    ref = Vector((1.0, 0.0, 0.0))
    b1 = ref - roll_dir * ref.dot(roll_dir); b1.normalize()
    b2 = roll_dir.cross(b1)
    for i in range(17):
        a = 2.0 * math.pi * i / 16
        q = p2 + b1 * (0.038 * math.cos(a)) + b2 * (0.038 * math.sin(a))
        ring_pts.append(tuple(q))
    ring = tube("tmp_rollring", ring_pts, 0.009, PAPER, seg=6)
    roll = merge_objs("PaperRoll", [roll, ring], PAPER)
    place(roll, left_arm, (0.0, 0.0, 0.0))

    # 右臂：红笔 + 文件夹
    pen = lathe("RedPen", [
        (-0.115, 0.0008, 0.0008, 0.0), (-0.085, 0.0095, 0.0095, 0.0),
        (-0.020, 0.0105, 0.0105, 0.0), (0.055, 0.0105, 0.0105, 0.0),
        (0.062, 0.0120, 0.0120, 0.0), (0.085, 0.0118, 0.0118, 0.0),
        (0.095, 0.0060, 0.0060, 0.0), (0.098, 0.0010, 0.0010, 0.0)],
        RED, seg=16, cap_bottom=False, cap_top=False)
    # 笔尖朝前下：绕 u 轴倾斜并移到握持位（右手局部）
    me = pen.data
    for vert in me.vertices:
        a = (vert.co.x, vert.co.z, vert.co.y)
        a = rot_u(a, 0.38, (0.0, 0.0, 0.0))
        vert.co = Vector(B(*a))
    me.update()
    place(pen, right_arm, (0.01, -0.93, 0.075))

    folder = beveled_box("PaperFolder", (0.27, 0.37, 0.028), (0.02, -0.55, 0.100),
                         PAPER, bevel=0.008, rot=('u', -0.12))
    place(folder, right_arm, (0.0, 0.0, 0.0))

    # ---------------- 钥匙串（Visual 局部 (0.27, 1.04, w=+0.05)） ----------------
    kc = empty("KeyChain", (0.27, 1.04, 0.05))
    parent_to(kc, visual) if visual is not None else None
    ring_pts = []
    for i in range(21):
        a = 2.0 * math.pi * i / 20
        ring_pts.append((0.027 * math.cos(a), 0.027 * math.sin(a), 0.0))
    kring = tube("Ring", ring_pts, 0.0055, KEYM, seg=6)
    place(kring, kc, (0.0, 0.0, 0.0))

    def build_key(name):
        parts = []
        bow_pts = []
        for i in range(17):
            a = 2.0 * math.pi * i / 16
            bow_pts.append((0.0135 * math.cos(a), -0.016 + 0.0135 * math.sin(a), 0.0))
        parts.append(tube(name + "_bow", bow_pts, 0.0042, KEYM, seg=6))
        parts.append(beveled_box(name + "_shaft", (0.009, 0.060, 0.004), (0.0, -0.062, 0.0),
                                 KEYM, bevel=0.0015))
        parts.append(beveled_box(name + "_t1", (0.011, 0.013, 0.004), (0.0095, -0.086, 0.0),
                                 KEYM, bevel=0.0012))
        parts.append(beveled_box(name + "_t2", (0.009, 0.011, 0.004), (0.0085, -0.068, 0.0),
                                 KEYM, bevel=0.0012))
        return merge_objs(name, parts, KEYM)

    k1 = build_key("Key1"); place(k1, kc, (0.012, -0.028, 0.0))
    k2 = build_key("Key2"); place(k2, kc, (0.062, -0.013, -0.015))
    k3 = build_key("Key3"); place(k3, kc, (-0.048, -0.008, 0.012))
    # Key2/Key3 的摆角（绕 w 轴，作用于各自几何，绕其挂点）
    for key_ob, ang, pivot in ((k2, 0.30, (0.062, -0.013, -0.015)), (k3, -0.26, (-0.048, -0.008, 0.012))):
        me = key_ob.data
        for vert in me.vertices:
            a = (vert.co.x, vert.co.z, vert.co.y)
            a = rot_w(a, ang, pivot)
            vert.co = Vector(B(*a))
        me.update()

    # ---------------- 头（枢轴 v=2.02） ----------------
    head = empty("Head", (0.0, 2.02, 0.0))
    parent_to(head, visual) if visual is not None else None

    neck = lathe("Neck", [(-0.075, 0.046, 0.046, 0.0), (0.020, 0.042, 0.042, 0.0),
                          (0.050, 0.048, 0.048, 0.0)], SKIN, seg=18)
    place(neck, head, (0.0, 0.0, 0.0))

    def head_shape(du, dv, dw):
        t = dv / 0.148
        u, v, w = du, dv, dw
        if t < 0.25:
            k = (0.25 - t) / 1.25
            u *= (1.0 - 0.30 * (k ** 1.3))
            if w > 0:
                w += 0.010 * (k ** 2) * (w / 0.112)
        if w > 0.06 and 0.25 < t < 0.55:
            w *= 0.985
        if w < -0.07:
            w *= 0.94
        if t > 0.8:
            v *= 0.97
        return (u, v, w)
    face = sphere("Face", (0.0, 0.115, 0.005), (0.104, 0.148, 0.112), SKIN,
                  shape_fn=head_shape, seg=40, rings=28)
    place(face, head, (0.0, 0.0, 0.0))

    for nm, su in (("EarL", -1.0), ("EarR", 1.0)):
        ear = sphere(nm, (su * 0.101, 0.100, 0.002), (0.010, 0.021, 0.015), SKIN, seg=16, rings=12)
        place(ear, head, (0.0, 0.0, 0.0))

    for nm, su in (("EyeL", -1.0), ("EyeR", 1.0)):
        eye = sphere(nm, (su * 0.0415, 0.128, 0.1005), (0.0273, 0.0151, 0.0116), EYED, seg=18, rings=12)
        place(eye, head, (0.0, 0.0, 0.0))
        brow = beveled_box(nm.replace("Eye", "Brow"), (0.054, 0.009, 0.012),
                           (su * 0.042, 0.160, 0.099), HAIR, bevel=0.003,
                           rot=('w', su * 0.16))
        place(brow, head, (0.0, 0.0, 0.0))

    nose = beveled_box("Nose", (0.034, 0.052, 0.030), (0.0, 0.082, 0.103),
                       SKIN, bevel=0.010, taper=(0.8, 1.05), rot=('u', 0.18))
    place(nose, head, (0.0, 0.0, 0.0))

    # 礼貌微笑：弧线 + 两端上扬小钩
    def face_w(u, v):
        val = 1.0 - (u / 0.104) ** 2 - ((v - 0.115) / 0.148) ** 2
        return 0.005 + 0.112 * math.sqrt(max(0.05, val)) - 0.004
    smile_pts = []
    for i in range(13):
        a = math.radians(195 + (345 - 195) * i / 12)
        u = 0.046 * math.cos(a)
        v = 0.052 + 0.046 * math.sin(a)
        smile_pts.append((u, v, face_w(u, v)))
    smile = tube("Smile", smile_pts, 0.0038, TRIM, seg=6)
    place(smile, head, (0.0, 0.0, 0.0))
    for nm, su, a0 in (("SmileCornerL", -1.0, 195), ("SmileCornerR", 1.0, 345)):
        a = math.radians(a0)
        u0, v0 = 0.046 * math.cos(a), 0.052 + 0.046 * math.sin(a)
        pts = [(u0, v0, face_w(u0, v0)),
               (u0 + su * 0.010, v0 + 0.007, face_w(u0 + su * 0.010, v0 + 0.007) - 0.002),
               (u0 + su * 0.017, v0 + 0.016, face_w(u0 + su * 0.017, v0 + 0.016) - 0.004)]
        corner = tube(nm, pts, 0.0034, TRIM, seg=6)
        place(corner, head, (0.0, 0.0, 0.0))

    # 眼镜：镜圈 + 鼻桥 + 镜腿
    for nm, su in (("GlassL", -1.0), ("GlassR", 1.0)):
        ring_pts = []
        for i in range(25):
            a = 2.0 * math.pi * i / 24
            ring_pts.append((su * 0.043 + 0.0375 * math.cos(a), 0.130 + 0.0375 * math.sin(a), 0.1135))
        parts = [tube(nm + "_ring", ring_pts, 0.0036, GLASSM, seg=6)]
        parts.append(tube(nm + "_temple", [(su * 0.0805, 0.132, 0.110), (su * 0.096, 0.131, 0.055),
                                           (su * 0.102, 0.128, 0.005)], 0.0028, GLASSM, seg=6))
        glass = merge_objs(nm, parts, GLASSM)
        place(glass, head, (0.0, 0.0, 0.0))
    bridge = tube("GlassesBridge", [(-0.021, 0.136, 0.1115), (0.0, 0.140, 0.1145), (0.021, 0.136, 0.1115)],
                  0.003, GLASSM, seg=6)
    place(bridge, head, (0.0, 0.0, 0.0))

    # 头发：穹顶 + 前额斜扫刘海 + 鬓角
    hair = lathe("Hair", [
        (0.148, 0.1125, 0.1195, 0.002), (0.168, 0.1135, 0.1205, 0.002),
        (0.205, 0.1115, 0.1185, 0.002), (0.245, 0.0985, 0.1025, 0.002),
        (0.272, 0.0640, 0.0660, 0.002), (0.284, 0.0140, 0.0140, 0.002)],
        HAIR, seg=36, cap_bottom=False)
    # 底边按方位塑形：前面抬高露额、后面压低盖颈
    me = hair.data
    for vert in me.vertices:
        a = (vert.co.x, vert.co.z, vert.co.y)  # 抽象 (u, v, w)，相对 Head 原点
        if a[1] < 0.175:
            ang = math.atan2(a[2], a[0])  # w 为前
            front = max(0.0, math.sin(ang))
            back = max(0.0, -math.sin(ang))
            a = (a[0], a[1] + 0.030 * front - 0.022 * back, a[2])
            vert.co = Vector(B(*a))
    me.update()
    place(hair, head, (0.0, 0.0, 0.0))

    fringe = beveled_box("HairFringe", (0.118, 0.030, 0.020), (0.014, 0.190, 0.098),
                         HAIR, bevel=0.008, rot=('w', -0.20))
    # 刘海并入 Hair 对象，保持原节点名集合（HairFringe 作为独立节点亦可，但并入更干净）
    hair_all = merge_objs("Hair", [hair, fringe], HAIR)
    place(hair_all, head, (0.0, 0.0, 0.0))

    for nm, su in (("HairSideL", -1.0), ("HairSideR", 1.0)):
        side = beveled_box(nm, (0.020, 0.088, 0.056), (su * 0.102, 0.104, 0.004),
                           HAIR, bevel=0.007, taper=(0.75, 1.0))
        place(side, head, (0.0, 0.0, 0.0))

    return visual

def main():
    argv = sys.argv[sys.argv.index("--") + 1:] if "--" in sys.argv else []
    out_dir = argv[0] if argv else "."
    os.makedirs(out_dir, exist_ok=True)
    build()
    out_path = os.path.join(out_dir, "teacher_model.glb")
    bpy.ops.object.select_all(action="SELECT")
    bpy.ops.export_scene.gltf(filepath=out_path, export_format="GLB",
                              export_apply=True, export_yup=True)
    print("TEACHER_MODEL_EXPORTED", out_path)

main()
