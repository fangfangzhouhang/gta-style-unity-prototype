"""Offline authoring utility. Writes explicit editable .tscn assets, never runs in game.
Original geometry and synthesized prototype audio. No imported third-party assets.
"""
from pathlib import Path
import json, math, random, struct, wave, re

ROOT = Path(__file__).resolve().parents[1]
for folder in ['scenes/chapters','scenes/actors','scenes/props','scenes/ui','assets/audio','resources','tests','evidence/logs','builds']:
    (ROOT/folder).mkdir(parents=True, exist_ok=True)

def q(s): return json.dumps(s, ensure_ascii=False)
def vec(x): return 'Vector3(%s)' % ', '.join(f'{v:.6g}' for v in x)
def color(c): return 'Color(%s)' % ', '.join(str(v) for v in c)

class Scene:
    def __init__(self): self.exts=[]; self.subs=[]; self.nodes=[]; self.ids=0; self.cache={}
    def ext(self,path,kind='Script'):
        key=(path,kind)
        if key in self.cache:return self.cache[key]
        idx=f'e{len(self.exts)+1}'
        self.exts.append(f'[ext_resource type="{kind}" path="res://{path}" id="{idx}"]')
        value=f'ExtResource("{idx}")';self.cache[key]=value;return value
    def sub(self,kind,**props):
        self.ids+=1; idx=f'r{self.ids}'
        self.subs.append(f'[sub_resource type="{kind}" id="{idx}"]\n'+'\n'.join(f'{k} = {v}' for k,v in props.items()))
        return f'SubResource("{idx}")'
    def node(self,name,kind='Node3D',parent=None,**props):
        header=f'[node name="{name}" type="{kind}"'+(f' parent="{parent}"' if parent is not None else '')+']'
        self.nodes.append(header+'\n'+'\n'.join(f'{k} = {v}' for k,v in props.items()))
        return name if parent=='.' or parent is None else parent+'/'+name
    def instance(self,name,path,parent='.',**props):
        ref=self.ext(path,'PackedScene')
        self.nodes.append(f'[node name="{name}" parent="{parent}" instance={ref}]\n'+'\n'.join(f'{k} = {v}' for k,v in props.items()))
        return name if parent=='.' else parent+'/'+name
    def save(self,path):
        text='[gd_scene load_steps=%d format=3]\n\n'%(len(self.exts)+len(self.subs)+1)+'\n\n'.join(self.exts+self.subs+self.nodes)+'\n'
        text=re.sub(r'(?<![\w.])(-?)\.(\d)',r'\g<1>0.\2',text)
        (ROOT/path).write_text(text,encoding='utf-8')
    def mat(self,name,c,rough=.8,metal=0,emission=None):
        key=('mat',name)
        if key in self.cache:return self.cache[key]
        if name in ['plaster','green','floor','wood']:
            shader=self.ext('assets/surface.gdshader','Shader')
            self.cache[key]=self.sub('ShaderMaterial',shader=shader,**{'shader_parameter/base_color':color(c),'shader_parameter/surface_kind':str(1 if name=='floor' else 2 if name=='wood' else 0)})
            return self.cache[key]
        props={'albedo_color':color(c),'roughness':str(rough),'metallic':str(metal)}
        if emission:props.update(emission_enabled='true',emission=color(emission),emission_energy_multiplier='1.3')
        self.cache[key]=self.sub('StandardMaterial3D',**props);return self.cache[key]
    def box(self,name,parent,pos,size,mat,layer=0):
        mesh=self.sub('BoxMesh',size=vec(size),material=mat)
        node=self.node(name,'MeshInstance3D',parent,position=vec(pos),mesh=mesh)
        if layer:
            shape=self.sub('BoxShape3D',size=vec(size))
            body=self.node('Body','StaticBody3D',node,collision_layer=str(layer),collision_mask='0')
            self.node('Shape','CollisionShape3D',body,shape=shape)
        return node
    def cylinder(self,name,parent,pos,r,h,mat,rot=None):
        mesh=self.sub('CylinderMesh',top_radius=str(r),bottom_radius=str(r),height=str(h),radial_segments='12',material=mat)
        props={'position':vec(pos),'mesh':mesh}
        if rot:props['rotation']=vec(rot)
        return self.node(name,'MeshInstance3D',parent,**props)
    def sphere(self,name,parent,pos,r,height,mat):
        mesh=self.sub('SphereMesh',radius=str(r),height=str(height),radial_segments='16',rings='8',material=mat)
        return self.node(name,'MeshInstance3D',parent,position=vec(pos),mesh=mesh)
    def text(self,name,parent,pos,text,size=.005,font=34,rot=0,tint=(.85,.84,.74,1)):
        f=self.ext('resources/chinese_font.tres','Font')
        return self.node(name,'Label3D',parent,position=vec(pos),rotation=vec((0,rot,0)),text=q(text),font=f,font_size=str(font),pixel_size=str(size),modulate=color(tint),outline_size='0',shaded='false',no_depth_test='false')

(ROOT/'resources/chinese_font.tres').write_text('[gd_resource type="SystemFont" format=3]\n\n[resource]\nfont_names = PackedStringArray("Microsoft YaHei", "Noto Sans CJK SC", "sans-serif")\n',encoding='utf8')
(ROOT/'resources/chapter_navigation.tres').write_text('''[gd_resource type="NavigationMesh" format=3]
[resource]
cell_size = 0.15
cell_height = 0.1
agent_height = 1.8
agent_radius = 0.3
agent_max_climb = 0.1
agent_max_slope = 35.0
region_min_size = 2.0
geometry_parsed_geometry_type = 1
geometry_collision_mask = 65
filter_baking_aabb = AABB(-11, -0.3, -9, 23, 3.25, 16)
''',encoding='utf8')

# Small original synthesized audio. Seeds fixed for reproducibility.
random.seed(40)
rate=22050
def audio(name,duration,fn):
    with wave.open(str(ROOT/'assets/audio'/name),'wb') as out:
        out.setnchannels(1);out.setsampwidth(2);out.setframerate(rate)
        out.writeframes(b''.join(struct.pack('<h',int(max(-1,min(1,fn(i/rate)))*24000)) for i in range(int(duration*rate))))
audio('step_soft.wav',.19,lambda t:(random.uniform(-1,1)*.23+math.sin(t*2*math.pi*95)*.25)*math.exp(-t*26))
audio('step_hard.wav',.24,lambda t:(random.uniform(-1,1)*.4+math.sin(t*2*math.pi*160)*.45)*math.exp(-t*25))
audio('bell.wav',.85,lambda t:(math.sin(t*2*math.pi*1250)+.42*math.sin(t*2*math.pi*1990))*.36*math.exp(-t*4)*(min(t*120,1)))
audio('tick.wav',.07,lambda t:random.uniform(-1,1)*.5*math.exp(-t*85))
audio('door.wav',.55,lambda t:(random.uniform(-1,1)*.3+math.sin(t*2*math.pi*(95+t*130))*.3)*math.sin(math.pi*t/.55)*.7)
audio('chain.wav',.6,lambda t:random.uniform(-1,1)*.6*max(0,math.sin(t*2*math.pi*17))*(1-t/.6))
audio('notice.wav',.45,lambda t:math.sin(2*math.pi*(330*t-55*t*t))*.28*math.sin(math.pi*t/.45))
notes=[659.25,523.25,587.33,392.0]
audio('chime.wav',7.6,lambda t:sum(math.sin(2*math.pi*f*(t-j*1.5))*.25*math.exp(-(t-j*1.5)*1.6) for j,f in enumerate(notes) if t>=j*1.5))

# Player scene.
s=Scene();s.node('Player','CharacterBody3D',script=s.ext('scripts/actors/player.gd'),collision_layer='2',collision_mask='53',floor_snap_length='0.2')
shape=s.sub('CapsuleShape3D',radius='0.26',height='1.75')
s.node('CollisionShape3D','CollisionShape3D','.',position=vec((0,.875,0)),shape=shape)
s.node('Head',parent='.',position=vec((0,1.62,0)))
s.node('Camera3D','Camera3D','Head',current='true',fov='78.0',near='0.04',far='90.0')
s.save('scenes/actors/player.tscn')

# Original stylized provisional teacher, explicit articulated pieces.
s=Scene();s.node('Teacher','CharacterBody3D',script=s.ext('scripts/actors/teacher.gd'),collision_layer='4',collision_mask='51',floor_snap_length='0.2',patrol_points='PackedVector3Array(-1.7,0.02,4.2, 2.7,0.02,4.2, 2.7,0.02,-3.1, -1.7,0.02,-3.1)')
s.node('CollisionShape3D','CollisionShape3D','.',position=vec((0,.9,0)),shape=s.sub('CapsuleShape3D',radius='.27',height='1.8'))
s.node('NavigationAgent3D','NavigationAgent3D','.',radius='.29',height='1.8',path_desired_distance='.25',target_desired_distance='.3')
s.node('Visual',parent='.')
cloth=s.mat('cloth',(.21,.27,.28,1));pants=s.mat('pants',(.12,.16,.18,1));skin=s.mat('skin',(.57,.43,.32,1));hair=s.mat('hair',(.12,.10,.09,1));shoe=s.mat('shoe',(.07,.08,.08,1));paper=s.mat('paper',(.75,.70,.57,1))
s.box('Torso','Visual',(0,1.18,0),(.46,.57,.26),cloth)
s.box('Collar','Visual',(0,1.47,-.06),(.26,.08,.24),paper)
for name,x in [('LeftLeg',-.12),('RightLeg',.12)]:
    s.node(name,parent='Visual',position=vec((x,.84,0)))
    s.box('Trousers','Visual/'+name,(0,-.36,0),(.16,.72,.19),pants)
    s.box('Shoe','Visual/'+name,(0,-.76,-.06),(.18,.14,.32),shoe)
for name,x in [('LeftArm',-.31),('RightArm',.31)]:
    s.node(name,parent='Visual',position=vec((x,1.43,0)))
    s.box('Sleeve','Visual/'+name,(0,-.24,0),(.14,.50,.17),cloth)
    s.box('Hand','Visual/'+name,(0,-.54,0),(.12,.16,.13),skin)
s.node('Head',parent='Visual',position=vec((0,1.64,0)))
s.sphere('Face','Visual/Head',(0,0,0),.145,.34,skin)
s.box('Hair','Visual/Head',(0,.12,.01),(.27,.1,.24),hair)
s.box('Nose','Visual/Head',(0,-.015,-.14),(.06,.08,.06),skin)
for x in [-.065,.065]:s.box('Eye'+str(x).replace('.','_'),'Visual/Head',(x,.025,-.136),(.045,.018,.018),shoe)
s.box('PaperFolder','Visual/RightArm',(0,-.40,-.14),(.28,.36,.035),paper)
s.save('scenes/actors/teacher.tscn')

# Reusable door; root x axis spans opening, +z is unlocking side.
s=Scene();s.node('Door',script=s.ext('scripts/props/door.gd'))
paint=s.mat('paint',(.28,.39,.34,1));trim=s.mat('trim',(.18,.25,.23,1));metal=s.mat('metal',(.57,.52,.35,1),.35,.6)
for x in [-.6,.6]:s.box('Frame'+str(x).replace('.','_'),'.',(x,1.15,0),(.10,2.3,.22),trim)
s.box('Lintel','.',(0,2.3,0),(1.3,.12,.22),trim)
s.node('Hinge',parent='.',position=vec((-.55,0,0)))
s.node('Panel','AnimatableBody3D','Hinge',collision_layer='16',collision_mask='0',sync_to_physics='false')
s.node('Shape','CollisionShape3D','Hinge/Panel',position=vec((.55,1.1,0)),shape=s.sub('BoxShape3D',size=vec((1.1,2.2,.07))))
s.box('Leaf','Hinge/Panel',(.55,1.1,0),(1.1,2.2,.07),paint)
s.box('Inset','Hinge/Panel',(.55,.72,.041),(.86,1.1,.018),trim)
for z in [-.065,.065]:s.box('Handle'+str(z).replace('.','_'),'Hinge/Panel',(.96,1.02,z),(.17,.04,.07),metal)
s.save('scenes/props/door.tscn')

# Timer.
s=Scene();s.node('ExamTimer',script=s.ext('scripts/props/exam_timer.gd'))
brass=s.mat('brass',(.62,.43,.19,1),.4,.35);face=s.mat('face',(.89,.84,.68,1));dark=s.mat('dark',(.12,.16,.16,1))
s.box('Case','.',(0,.04,0),(.19,.15,.10),brass)
s.box('Face','.',(0,.045,.055),(.16,.11,.012),face)
s.cylinder('Winder','.',(0,.14,0),.025,.035,brass)
s.text('Display','.',(0,.04,.065),'03',.0019,30,tint=(.12,.16,.16,1))
s.node('Target','StaticBody3D','.',collision_layer='8',collision_mask='0')
s.node('Shape','CollisionShape3D','Target',position=vec((0,.04,0)),shape=s.sub('BoxShape3D',size=vec((.27,.25,.24))))
s.save('scenes/props/exam_timer.tscn')

# Reusable school desk and chair in real dimensions.
s=Scene();s.node('Desk')
wood=s.mat('wood',(.45,.29,.16,1),.65);steel=s.mat('steel',(.16,.23,.22,1),.48,.3)
s.box('Top','.',(0,.74,0),(.68,.045,.48),wood)
s.box('Storage','.',(0,.64,0),(.59,.13,.41),steel)
for x in [-.27,.27]:
    for z in [-.17,.17]:s.cylinder('Leg'+str(x)+str(z),'.',(x,.31,z),.018,.62,steel)
s.node('Body','StaticBody3D','.',collision_layer='1',collision_mask='0')
s.node('Shape','CollisionShape3D','Body',position=vec((0,.38,0)),shape=s.sub('BoxShape3D',size=vec((.68,.76,.48))))
s.box('Seat','.',(0,.44,.60),(.37,.04,.37),wood)
s.box('Back','.',(0,.70,.79),(.39,.30,.035),wood)
for x in [-.14,.14]:
    for z in [.46,.74]:s.cylinder('ChairLeg'+str(x)+str(z),'.',(x,.22,z),.016,.44,steel)
s.node('ChairBody','StaticBody3D','.',collision_layer='1',collision_mask='0')
s.node('Shape','CollisionShape3D','ChairBody',position=vec((0,.44,.62)),shape=s.sub('BoxShape3D',size=vec((.4,.88,.4))))
s.save('scenes/props/desk.tscn')

# UI authored as an ordinary editable scene.
s=Scene();s.node('HUD','CanvasLayer',script=s.ext('scripts/ui/hud.gd'),layer='10')
style=s.sub('StyleBoxFlat',bg_color=color((.06,.085,.09,.97)),border_width_left='1',border_width_top='1',border_width_right='1',border_width_bottom='1',border_color=color((.35,.39,.35,.8)),corner_radius_top_left='8',corner_radius_top_right='8',corner_radius_bottom_left='8',corner_radius_bottom_right='8')
button=s.sub('StyleBoxFlat',bg_color=color((.16,.22,.21,1)),content_margin_left='16.0',content_margin_right='16.0',content_margin_top='12.0',content_margin_bottom='12.0',corner_radius_top_left='4',corner_radius_top_right='4',corner_radius_bottom_left='4',corner_radius_bottom_right='4')
hover=s.sub('StyleBoxFlat',bg_color=color((.32,.38,.29,1)),content_margin_left='16.0',content_margin_right='16.0',content_margin_top='12.0',content_margin_bottom='12.0')
theme=s.sub('Theme',default_font=s.ext('resources/chinese_font.tres','Font'),default_font_size='18',**{'Button/styles/normal':button,'Button/styles/hover':hover,'Button/styles/pressed':hover,'Button/styles/focus':hover,'Label/colors/font_color':color((.87,.88,.81,1))})
def ctrl(name,kind,parent,x,y,w,h,**props):
    return s.node(name,kind,parent,offset_left=str(x),offset_top=str(y),offset_right=str(x+w),offset_bottom=str(y+h),theme=theme,**props)
def label(name,parent,x,y,w,h,text,size=18,**props):
    return ctrl(name,'Label',parent,x,y,w,h,text=q(text),**{'theme_override_font_sizes/font_size':str(size)},**props)
def panel(name,x,y,w,h):
    return ctrl(name,'Panel','.',x,y,w,h,visible='false',**{'theme_override_styles/panel':style})
label('Chapter','.',30,24,400,38,'最后一遍铃  /  试走版',17)
label('Crosshair','.',630,385,20,22,'·',26,horizontal_alignment='1')
label('Prompt','.',280,505,720,50,'',19,horizontal_alignment='1')
label('Inventory','.',30,718,900,28,'',17)
label('Help','.',30,754,1000,24,'Shift 跑  ·  Ctrl 蹲  ·  Tab 手册  ·  Esc 暂停  ·  F8 截图',15,modulate=color((.7,.75,.7,.8)))
label('Toast','.',260,610,760,78,'',19,horizontal_alignment='1',autowrap_mode='2')
label('Sound','.',340,86,600,32,'',17,horizontal_alignment='1')
panel('Menu',70,104,450,585);s.node('Box','Control','Menu')
label('Title','Menu/Box',32,30,380,64,'逃离考场',40)
label('Subtitle','Menu/Box',34,105,380,80,'',19)
for i,(name,text) in enumerate([('Resume','继续'),('New','开始新的试走'),('Journal','已发现的事实'),('Settings','声音与操作'),('Quit','保存并退出')]):
    ctrl(name,'Button','Menu/Box',32,200+i*57,386,46,text=q(text))
label('Footer','Menu/Box',34,502,380,64,'',14)
panel('Reading',270,95,740,610);s.node('Box','Control','Reading')
label('Title','Reading/Box',30,25,680,45,'',29)
ctrl('Body','RichTextLabel','Reading/Box',30,90,680,400,text='""',scroll_active='true',**{'theme_override_font_sizes/normal_font_size':'20'})
ctrl('Hint','Button','Reading/Box',30,535,290,46,text=q('主动请求下一层提示'))
ctrl('Close','Button','Reading/Box',500,535,210,46,text=q('合上  /  Esc'))
panel('Settings',350,120,580,560);s.node('Box','Control','Settings')
label('Title','Settings/Box',30,25,500,45,'声音与操作',29)
for i,(name,text,minv,maxv) in enumerate([('Sensitivity','鼠标灵敏度',6,50),('Fov','视野角度',65,95),('Volume','主音量',-30,0)]):
    label(name+'Label','Settings/Box',30,100+i*100,500,26,text)
    ctrl(name,'HSlider','Settings/Box',30,140+i*100,500,24,min_value=str(minv),max_value=str(maxv),step='1')
ctrl('Captions','CheckButton','Settings/Box',30,413,500,40,text=q('显示可听声音的文字提示'))
ctrl('Close','Button','Settings/Box',30,485,500,46,text=q('保存并返回'))
panel('Recovery',0,0,1280,800);label('Label','Recovery',240,300,800,200,'',25,horizontal_alignment='1')
panel('Ending',240,150,800,500);s.node('Box','Control','Ending')
label('Title','Ending/Box',30,30,740,70,'考试结束了。',38,horizontal_alignment='1')
label('Body','Ending/Box',40,115,720,275,'',21,horizontal_alignment='1')
ctrl('TitleButton','Button','Ending/Box',230,420,340,48,text=q('回到标题'))
panel('Confirm',410,270,460,255);s.node('Box','Control','Confirm')
label('Text','Confirm/Box',25,25,410,100,'开始新的一次试走？\n这会替换本章的当前进度。',21,horizontal_alignment='1')
ctrl('Yes','Button','Confirm/Box',25,170,195,48,text=q('开始新进度'))
ctrl('No','Button','Confirm/Box',240,170,195,48,text=q('保留并返回'))
s.save('scenes/ui/hud.tscn')

# Chapter: one compact authored wing, no runtime geometry construction.
s=Scene();s.node('Chapter01',script=s.ext('scripts/systems/chapter.gd'))
plaster=s.mat('plaster',(.66,.65,.57,1));green=s.mat('green',(.32,.43,.37,1),.74);floor=s.mat('floor',(.30,.34,.31,1),.64)
ceiling=s.mat('ceiling',(.70,.70,.64,1));trim=s.mat('trim',(.20,.26,.23,1));wood=s.mat('wood',(.43,.28,.15,1),.6)
paper=s.mat('paper',(.78,.75,.62,1));steel=s.mat('steel',(.22,.28,.29,1),.45,.45);blackboard=s.mat('board',(.075,.14,.12,1));brass=s.mat('brass',(.62,.44,.21,1),.43,.4)
black=s.mat('black',(.045,.07,.075,1));light=s.mat('light',(.85,.88,.77,1),emission=(.65,.71,.55,1));red=s.mat('red',(.45,.20,.15,1))
glass=s.sub('StandardMaterial3D',transparency='1',albedo_color=color((.53,.63,.64,.16)),roughness='.35',metallic_specular='.12',cull_mode='2')
env=s.sub('Environment',background_mode='1',background_color=color((.30,.39,.43,1)),ambient_light_source='2',ambient_light_color=color((.61,.68,.66,1)),ambient_light_energy='.55',reflected_light_source='1',tonemap_mode='2',tonemap_exposure='1.0',fog_enabled='false')
s.node('WorldEnvironment','WorldEnvironment','.',environment=env)
s.node('Sun','DirectionalLight3D','.',rotation=vec((-.9,-.7,0)),light_color=color((.70,.80,.89,1)),light_energy='.7',shadow_enabled='true',directional_shadow_max_distance='30')
s.node('Architecture','NavigationRegion3D','.',navigation_mesh=s.ext('resources/chapter_navigation.tres','NavigationMesh'))
rooms={'C':(-10,-8,-3,2.5),'M':(-3,-8,4,-2),'W':(-3,-2,-.5,3),'E':(1.5,-2,4,3),'J':(-3,3,4,5.5),'B':(4,-5,8,.5),'G':(4,.5,8,3.4),'O':(8,.5,11,3.4)}
for name,(x0,z0,x1,z1) in rooms.items():
    p=s.node('Room'+name,parent='Architecture')
    s.box('Floor',p,((x0+x1)/2,-.10,(z0+z1)/2),(x1-x0,.20,z1-z0),floor,1)
    s.box('Ceiling',p,((x0+x1)/2,3.35,(z0+z1)/2),(x1-x0,.16,z1-z0),ceiling,1)
    # floor seams: sparse, non-coplanar inset strips, no overlapping floors
    for index,z in enumerate(range(math.ceil(z0),math.floor(z1),2)):
        s.box('Joint'+str(index),p,((x0+x1)/2,.001,z),(x1-x0,.001,.012),trim)

wall_index=0
def wall(axis,fixed,start,end,openings=(),windows=()):
    global wall_index
    cuts=sorted([(c-w/2,c+w/2,'door') for c,w in openings]+[(c-w/2,c+w/2,'window') for c,w in windows])
    spans=[];cursor=start
    for a,b,kind in cuts:
        if a>cursor:spans.append((cursor,a,'solid'))
        spans.append((a,b,kind));cursor=b
    if cursor<end:spans.append((cursor,end,'solid'))
    def piece(name,a,b,y,h,mat,depth=.18,layer=1):
        size=(b-a,h,depth) if axis=='x' else (depth,h,b-a)
        pos=((a+b)/2,y,fixed) if axis=='x' else (fixed,y,(a+b)/2)
        return s.box(name,node,pos,size,mat,layer)
    for a,b,kind in spans:
        wall_index+=1;node=s.node('Wall%02d'%wall_index,parent='Architecture')
        if kind=='solid':
            piece('Lower',a,b,.575,1.15,green);piece('Upper',a,b,2.225,2.15,plaster)
            piece('Base',a,b,.055,.11,trim,.2,0)
            piece('DadoCap',a,b,1.15,.025,trim,.2,0)
        elif kind=='door':
            piece('Header',a,b,2.8,1,plaster)
        else:
            piece('SillWall',a,b,.48,.96,green);piece('TopWall',a,b,3.02,.56,plaster)
            piece('Glass',a,b,1.85,1.77,glass,.025,32)
            piece('Sill',a-.06,b+.06,.97,.07,paper,.30,0)
            for i,c in enumerate([a,(a+b)/2,b]):piece('Frame'+str(i),c-.025,c+.025,1.85,1.83,steel,.09,0)
            piece('FrameTop',a,b,2.74,.05,steel,.09,0)
wall('x',-8,-10,4)
wall('z',-10,-8,2.5,windows=[(-5.4,2.0),(-1,2.1)])
wall('x',2.5,-10,-3)
wall('z',-3,-8,5.5,openings=[(-5.5,1.2),(1.1,1.2)])
wall('x',-2,-3,4,openings=[(-1.75,1.25),(2.75,1.25)])
wall('z',-.5,-2,3);wall('z',1.5,-2,3);wall('x',3,-.5,1.5)
wall('x',5.5,-3,4,windows=[(.5,2.2)])
wall('z',4,-8,5.5,openings=[(-3.6,1.2),(-.7,1.2),(1.8,1.5)])
wall('x',-5,4,8);wall('z',8,-5,.5,windows=[(-2.4,2)])
wall('x',.5,4,8);wall('x',3.4,4,11)
wall('z',8,.5,3.4,openings=[(1.85,2.1)])
wall('z',11,.5,3.4);wall('x',.5,8,11)
s.box('ShortcutBakeCut','Architecture',(-3,1.2,-5.5),(.5,2.4,1.5),trim,64)
# Its visible mesh is hidden, collider is only used for navigation baking.
s.nodes[-3]=s.nodes[-3]+'\nvisible = false' # mesh entry precedes Body, Shape
s.node('Furniture',parent='Architecture')
for i,x in enumerate([-8.5,-6.8,-4.6]):
    for j,z in enumerate([-3.0,-1.2,.6]):
        s.instance('Desk%d%d'%(i,j),'scenes/props/desk.tscn','Architecture/Furniture',position=vec((x,0,z)))
        s.box('Paper%d%d'%(i,j),'Architecture/Furniture',(x,.768,z),(.23,.006,.31),paper)
s.box('Lectern','Architecture/Furniture',(-6.5,.4,-6.85),(1.9,.8,.65),wood,1)
s.box('FrontPanel','Architecture/Furniture',(-6.5,.45,-6.51),(1.55,.48,.04),green)
s.box('BoardFrame','Architecture',(-6.5,1.95,-7.86),(4.2,1.4,.1),wood)
s.box('Blackboard','Architecture',(-6.5,1.95,-7.79),(4,1.23,.04),blackboard)
s.text('BoardWriting','Architecture',(-6.5,2.02,-7.755),'保持安静\n终场铃响后离场',.008,32)
s.box('CoverCabinet','Architecture/Furniture',(-4.0,.77,-.8),(.6,1.54,1.35),green,1)
for y in [.25,.62,1.0,1.37]:s.box('CabinetShelf'+str(y),'Architecture/Furniture',(-4.32,y,-.8),(.04,.035,1.2),wood)
s.box('PrintDivider','Architecture/Furniture',(.4,.9,-4.8),(.45,1.8,2.0),steel,1)
for i,y in enumerate([.15,.55,.95,1.35,1.7]):
    s.box('PaperShelf'+str(i),'Architecture/Furniture',(.4,y,-4.8),(.8,.045,2),wood)
    for j,z in enumerate([-5.3,-4.7,-4.1]):s.box('PaperStack%d%d'%(i,j),'Architecture/Furniture',(.4,y+.12,z),(.57,.20,.42),paper)
s.box('Printer','Architecture/Furniture',(2.5,.6,-6.6),(1.4,1.2,.9),steel,1)
s.box('PrinterTop','Architecture/Furniture',(2.5,1.22,-6.6),(1.3,.08,.85),black)
s.box('PrintDesk','Architecture/Furniture',(-1.6,.40,-7.1),(1.7,.8,.6),wood,1)
s.text('PrintWallText','Architecture',(.5,2.2,-7.88),'印务  /  重印，不必填写姓名',.006,28)
s.box('BroadcastDesk','Architecture/Furniture',(7.45,.40,-2.6),(.8,.8,3.0),wood,1)
s.box('BroadCover','Architecture/Furniture',(5.6,.77,-4.25),(1.3,1.54,.45),green,1)
s.box('Sink','Architecture/Furniture',(-2.5,.86,4.7),(.65,.18,.65),paper,1)
s.box('SinkBase','Architecture/Furniture',(-2.5,.4,4.7),(.4,.8,.45),green,1)
s.cylinder('Tap','Architecture/Furniture',(-2.65,1.05,4.7),.025,.25,steel)
s.text('ExitDirection','Architecture',(1.42,2.15,1.65),'离场出口 →',.007,28,math.pi/2)
s.text('WClockLabel','Architecture',(-2.88,2.1,-.5),'考场\n←',.005,28,math.pi/2)
s.text('PrintSign','Architecture',(-1.75,2.56,-1.87),'印务室',.004,26)
s.text('EastSign','Architecture',(2.75,2.56,-1.87),'印务室',.004,26)
s.text('BSign','Architecture',(3.88,2.6,-.7),'播音',.004,26,-math.pi/2)
s.text('ExitSign','Architecture',(7.84,2.8,1.85),'安 全 出 口',.004,26,-math.pi/2,tint=(.65,.88,.69,1))
s.text('ShortcutSign','Architecture',(-3.11,2.55,-5.5),'运卷门',.004,25,-math.pi/2)
s.text('GInstruction','Architecture',(6.6,1.6,3.28),'三档棘轮\n摇升一档，松手可停',.004,27,math.pi)
s.node('Lighting',parent='.')
for i,(x,z,energy,tint) in enumerate([(-7.8,-5.8,1.5,(1,.88,.67,1)),(-5.0,-.5,1.3,(.93,.95,.83,1)),(-1.5,-5.7,1.2,(.82,.91,.90,1)),(2.7,-4.8,1.1,(.82,.91,.90,1)),(-1.7,1.3,1.0,(.92,.94,.78,1)),(2.7,1.2,1.0,(.78,.88,.93,1)),(.4,4.4,1.0,(.72,.85,.93,1)),(6.4,-2.5,1.1,(.75,.85,.90,1)),(6.3,1.8,1.0,(.82,.87,.80,1))]):
    fixture=s.node('Fixture'+str(i),parent='Lighting',position=vec((x,3.15,z)))
    s.box('Housing',fixture,(0,0,0),(1.15,.08,.23),steel)
    for k,dz in enumerate([-.065,.065]):s.cylinder('Tube'+str(k),fixture,(0,-.06,dz),.022,1.0,light,(0,0,math.pi/2))
    s.node('Light','OmniLight3D',fixture,position=vec((0,-.25,0)),light_color=color(tint),light_energy=str(energy*.8),light_size='.3',omni_range='5.8',omni_attenuation='1.3',shadow_enabled='true',shadow_bias='.035')
s.node('WindowBackdrop',parent='.')
concrete=s.mat('concrete',(.38,.41,.38,1))
leaves=s.mat('leaves',(.16,.26,.19,1))
s.box('Garden','WindowBackdrop',(-15,-.22,-2),(9,.2,18),concrete)
s.box('FarWall','WindowBackdrop',(-19,1,-2),(.3,2,18),plaster)
for i,z in enumerate([-6,-1,4]):
    s.cylinder('Trunk'+str(i),'WindowBackdrop',(-14.5,1.2,z),.12,2.4,wood)
    s.sphere('Crown'+str(i),'WindowBackdrop',(-14.5,3,z),1.1,2.3,leaves)
s.box('EastWall','WindowBackdrop',(13,1,-2),(.3,2,12),plaster)
s.node('Doors',parent='.')
s.instance('Classroom','scenes/props/door.tscn','Doors',position=vec((-3,0,1.1)),rotation=vec((0,-math.pi/2,0)),door_name=q('考场门'),initial_open='true')
s.instance('Print','scenes/props/door.tscn','Doors',position=vec((-1.75,0,-2)),door_name=q('印务门'),initial_open='true')
s.instance('Shortcut','scenes/props/door.tscn','Doors',position=vec((-3,0,-5.5)),rotation=vec((0,math.pi/2,0)),door_name=q('运卷门'),is_shortcut='true')
s.node('NavigationLink3D','NavigationLink3D','Doors/Shortcut',enabled='false',start_position=vec((0,.1,-.9)),end_position=vec((0,.1,.9)))
s.instance('Player','scenes/actors/player.tscn',position=vec((-6.5,.04,-5.1)))
s.instance('Teacher','scenes/actors/teacher.tscn',position=vec((-1.7,.04,4.2)),rotation=vec((0,-math.pi/2,0)))
s.instance('ExamTimer','scenes/props/exam_timer.tscn',position=vec((-6.5,.84,-6.65)))
s.node('Checkpoints',parent='.')
s.node('Classroom','Marker3D','Checkpoints',position=vec((-6.5,.04,-5.1)))
s.node('Print','Marker3D','Checkpoints',position=vec((2.7,.04,-4.0)))
s.node('Speakers',parent='.')
for name,p in [('Classroom',(-7,2.65,-7.6)),('Print',(1.5,2.65,-7.6)),('East',(3.65,2.65,1.5))]:
    root=s.node(name,'Marker3D','Speakers',position=vec(p))
    s.box('Box',root,(0,0,0),(.38,.28,.16),wood)
    for i in range(5):s.box('Grille'+str(i),root,(0,-.10+i*.05,.09),(.32,.018,.01),black)
s.node('Gate',parent='.',position=vec((8,0,1.85)),rotation=vec((0,math.pi/2,0)),script=s.ext('scripts/props/ratchet_gate.gd'))
s.node('GateBody','AnimatableBody3D','Gate',collision_layer='16',collision_mask='0',sync_to_physics='false')
s.node('Shape','CollisionShape3D','Gate/GateBody',position=vec((0,1.3,0)),shape=s.sub('BoxShape3D',size=vec((2.1,2.6,.12))))
for i in range(13):s.box('Slat'+str(i),'Gate/GateBody',(0,.1+i*.2,0),(2.1,.18,.1),steel)
s.box('BottomRail','Gate/GateBody',(0,.04,0),(2.15,.07,.15),brass)
for j in range(16):s.cylinder('Chain'+str(j),'Gate',(-1.17,.35+j*.1,-.32),.018,.07,brass,(0,0,(j%2)*math.pi/2))
s.node('Handle','StaticBody3D','Gate',position=vec((-1.17,1.1,-.32)),collision_layer='8',collision_mask='0')
s.node('Shape','CollisionShape3D','Gate/Handle',shape=s.sub('BoxShape3D',size=vec((.3,.8,.3))))
s.text('Status','Gate',(-1.17,2.2,-.35),'已升起 0 / 3 档',.003,25,math.pi)
s.node('Broadcast',parent='.',position=vec((7.79,1.4,-2.6)),rotation=vec((0,-math.pi/2,0)),script=s.ext('scripts/props/broadcast.gd'))
s.box('Back','Broadcast',(0,0,0),(2.7,.9,.09),steel)
s.text('Heading','Broadcast',(0,.60,.07),'终场信号  →  输出方式  →  分区',.0035,27)
for name,act,x in [('Stop','stop',-.99),('Mode','mode',-.33),('Zone','zone',.33),('Test','test',.99)]:
    root=s.node(name,parent='Broadcast',position=vec((x,0,.08)),script=s.ext('scripts/props/panel_button.gd'),action=q(act))
    s.box('Switch',root,(0,0,0),(.48,.20,.08),red if act=='stop' else brass)
    s.node('Target','StaticBody3D',root,collision_layer='8',collision_mask='0')
    s.node('Shape','CollisionShape3D',root+'/Target',shape=s.sub('BoxShape3D',size=vec((.55,.48,.15))))
    s.text('Label',root,(0,-.25,.05),'控制',.0027,24)
s.node('LoopAudio','AudioStreamPlayer','Broadcast',stream=s.ext('assets/audio/chime.wav','AudioStream'),volume_db='-24')

def note(name,pos,yaw,title,body,fact,summary):
    root=s.node(name,parent='.',position=vec(pos),rotation=vec((0,yaw,0)),script=s.ext('scripts/props/note.gd'),fact_id=q(fact),title=q(title),body=q(body),summary=q(summary))
    s.box('Paper',root,(0,0,0),(.48,.62,.012),paper)
    s.text('Caption',root,(0,.08,.012),title+'\n\nE  查看',.0025,25,tint=(.16,.2,.17,1))
    s.node('Target','StaticBody3D',root,collision_layer='8',collision_mask='0')
    s.node('Shape','CollisionShape3D',root+'/Target',shape=s.sub('BoxShape3D',size=vec((.5,.65,.05))))
note('ExamSheet',(-7.8,1.35,-7.73),0,'终场须知','考试已经结束时，请听终场铃。\n\n可走动交卷，不必等在座位。\n门外贴着离场方向。\n\n纸角有人写：\n「如果那一分钟永远不走呢？」','exam','终场须知允许交卷。考场正门没有上锁，出口在东廊。')
note('PrintRecord',(-1.55,1.55,-7.86),0,'检修便签','播音柜：左侧是重播开关。\n\n监听只进本机，外放才送到分区。\n教室、印务、东廊，各接自己的喇叭。\n\n下方另一行字被划掉：\n「终场通知无需重印。」','wiring','检修记录：监听只在本机响；外放按分区旋钮送到对应喇叭。')
s.node('Acoustics',parent='.',script=s.ext('scripts/systems/acoustics.gd'))
s.node('Zones',parent='Acoustics');s.node('Portals',parent='Acoustics')
for name,(x0,z0,x1,z1) in rooms.items():
    s.node(name,'Marker3D','Acoustics/Zones',**{'metadata/bounds':f'Rect2({x0}, {z0}, {x1-x0}, {z1-z0})'})
for name,a,b,p,door in [('CW','C','W',(-3,1,1.1),'Doors/Classroom'),('CM','C','M',(-3,1,-5.5),'Doors/Shortcut'),('WM','W','M',(-1.75,1,-2),'Doors/Print'),('EM','E','M',(2.75,1,-2),None),('WJ','W','J',(-1.75,1,3),None),('EJ','E','J',(2.75,1,3),None),('MB','M','B',(4,1,-3.6),None),('EB','E','B',(4,1,-.7),None),('EG','E','G',(4,1,1.8),None),('GO','G','O',(8,1,1.85),None)]:
    props={'position':vec(p),'metadata/a':q(a),'metadata/b':q(b)}
    if door:props['metadata/door']=q(door)
    s.node(name,'Marker3D','Acoustics/Portals',**props)
s.instance('HUD','scenes/ui/hud.tscn')
s.save('scenes/chapters/chapter_01.tscn')
print('Authored explicit chapter, 5 reusable scenes, UI, font resource and 8 original prototype WAV files.')
