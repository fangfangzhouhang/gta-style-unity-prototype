using UnityEngine;

/// <summary>Balanced commuter motorcycle: metre units, physical collisions and first-person riding.</summary>
[RequireComponent(typeof(Rigidbody))]
public class CampusMotorcycle : MonoBehaviour
{
    public ThirdPersonController player;
    public ThirdPersonCamera cameraRig;
    public Transform visual, frontWheel, rearWheel;
    public float topSpeed=25f, acceleration=3.5f, brakeDeceleration=7f;
    [Tooltip("1.30 = 30% more yaw at the same speed and steering input.")]
    public float steeringMultiplier=1.30f;
    public bool IsRiding { get; private set; }
    public float SpeedKmh { get { return body==null?0:new Vector2(body.velocity.x,body.velocity.z).magnitude*3.6f; } }
    Rigidbody body;
    Transform previousParent;
    Vector3 parkingPosition;
    Quaternion parkingRotation;
    float throttle,steer,lookYaw,lookPitch,lockout;
    bool brake;
    public bool UseExternalInput { get; set; }
    public void SetDriveInput(float gas,float turn,bool stop)
    {throttle=Mathf.Clamp(gas,-1,1);steer=Mathf.Clamp(turn,-1,1);brake=stop;}
    CampusFlowController flow;
    WorldMapUI map;
    public bool RideUnlocked { get { var chapter=UnityCompat.FindOne<CampusDepartureChapter>();return chapter==null || chapter.State.departed; } }
    public bool CanMount { get { return RideUnlocked && !IsRiding && player!=null && Vector3.Distance(player.transform.position,transform.position+Vector3.up*.7f)<2.6f; } }
    void Awake()
    {
        body=GetComponent<Rigidbody>();body.mass=220;body.drag=.04f;body.angularDrag=4;
        body.constraints=RigidbodyConstraints.FreezeRotationX|RigidbodyConstraints.FreezeRotationZ;
        body.interpolation=RigidbodyInterpolation.Interpolate;body.collisionDetectionMode=CollisionDetectionMode.ContinuousDynamic;
        body.centerOfMass=new Vector3(0,.35f,0);
        // Grip is applied by the controller. Static friction on locked, spherical
        // wheel contacts would otherwise prevent a low-powered commuter starting.
        var contact=new PhysicMaterial("MotorcycleContact"){dynamicFriction=0,staticFriction=0,bounciness=0,frictionCombine=PhysicMaterialCombine.Minimum};
        foreach(var col in GetComponentsInChildren<Collider>())col.sharedMaterial=contact;
        parkingPosition=transform.position;parkingRotation=transform.rotation;
    }
    void Start(){flow=UnityCompat.FindOne<CampusFlowController>();map=UnityCompat.FindOne<WorldMapUI>();if(cameraRig==null)cameraRig=UnityCompat.FindOne<ThirdPersonCamera>();}
    bool Blocked { get { return Time.timeScale==0 || (PauseMenu.Instance!=null && PauseMenu.Instance.IsOpen) || (map!=null && map.IsOpen) || (flow!=null && flow.IsBusy); } }
    void Update()
    {
        lockout-=Time.unscaledDeltaTime;
        if(Blocked){throttle=steer=0;brake=true;return;}
        if(Input.GetKeyDown(KeyCode.F) && lockout<=0){if(IsRiding)TryDismount();else TryMount();lockout=.4f;}
        if(!UseExternalInput){throttle=IsRiding?Input.GetAxis("Vertical"):0;steer=IsRiding?Input.GetAxis("Horizontal"):0;brake=!IsRiding||Input.GetKey(KeyCode.Space);}
        if(IsRiding && Cursor.lockState==CursorLockMode.Locked)
        {lookYaw=Mathf.Clamp(lookYaw+Input.GetAxis("Mouse X")*2,-80,80);lookPitch=Mathf.Clamp(lookPitch-Input.GetAxis("Mouse Y")*2,-65,65);}
    }
    public static Vector3 LimitedVelocity(Vector3 velocity,float limit)
    {Vector3 flat=new Vector3(velocity.x,0,velocity.z);return Vector3.ClampMagnitude(flat,limit)+Vector3.up*velocity.y;}
    void FixedUpdate()
    {
        if(body==null)return;
        Vector3 flat=new Vector3(body.velocity.x,0,body.velocity.z);
        bool grounded=GroundAt(transform.position+Vector3.up*.75f,1.3f,out RaycastHit ground);
        if(grounded)
        {
            Vector3 forward=Vector3.ProjectOnPlane(transform.forward,ground.normal).normalized;
            if(brake || throttle<-.01f)
            {flat=Vector3.MoveTowards(flat,Vector3.zero,brakeDeceleration*Time.fixedDeltaTime);body.velocity=new Vector3(flat.x,body.velocity.y,flat.z);}
            else if(throttle>.01f && flat.magnitude<topSpeed-.1f)body.AddForce(forward*throttle*acceleration,ForceMode.Acceleration);
            else body.AddForce(-flat.normalized*Mathf.Min(flat.magnitude/Time.fixedDeltaTime,.45f),ForceMode.Acceleration);
            body.AddForce(-transform.right*Vector3.Dot(body.velocity,transform.right)*10,ForceMode.Acceleration);
            if(IsRiding && flat.magnitude>.3f)
            {
                float yawRate=Mathf.Min(65,Mathf.Rad2Deg*flat.magnitude/Mathf.Lerp(3,30,Mathf.Clamp01(flat.magnitude/topSpeed)));
                body.MoveRotation(body.rotation*Quaternion.Euler(0,steer*yawRate*steeringMultiplier*Time.fixedDeltaTime,0));
            }
            if(visual!=null)
            {float pitch=Vector3.SignedAngle(transform.forward,forward,transform.right);
             visual.localRotation=Quaternion.Slerp(visual.localRotation,Quaternion.Euler(pitch,0,-steer*Mathf.Min(flat.magnitude,12)),.15f);}
        }
        body.velocity=LimitedVelocity(body.velocity,topSpeed);
        float spin=flat.magnitude/.32f*Mathf.Rad2Deg*Time.fixedDeltaTime;
        if(frontWheel!=null)frontWheel.Rotate(0,-spin,0,Space.Self);
        if(rearWheel!=null)rearWheel.Rotate(0,-spin,0,Space.Self);
        if(transform.position.y< -7 || transform.position.x< -108 || transform.position.x>128 || transform.position.z< -158 || transform.position.z>238)
        {
            bool riding=IsRiding;
            if(riding)ReleasePlayer(parkingPosition+Vector3.left*1.3f+Vector3.up*.92f);
            body.position=parkingPosition;body.rotation=parkingRotation;body.velocity=Vector3.zero;body.angularVelocity=Vector3.zero;
            if(riding && flow!=null)flow.RecoverFromFall();
        }
    }
    bool GroundAt(Vector3 start,float range,out RaycastHit nearest)
    {
        nearest=default(RaycastHit);float distance=float.MaxValue;
        foreach(var hit in Physics.RaycastAll(start,Vector3.down,range,~0,QueryTriggerInteraction.Ignore))
        {
            if(hit.transform.IsChildOf(transform) || (player!=null && hit.transform.IsChildOf(player.transform)) || hit.normal.y<.7f)continue;
            if(hit.distance<distance){nearest=hit;distance=hit.distance;}
        }
        return distance<float.MaxValue;
    }
    public bool TryMount()
    {
        if(!CanMount || Blocked)return false;
        previousParent=player.transform.parent;player.controlEnabled=false;player.GetComponent<CharacterController>().enabled=false;
        player.transform.SetParent(transform,true);player.transform.localPosition=new Vector3(0,1,0);
        foreach(var renderer in player.GetComponentsInChildren<Renderer>())renderer.enabled=false;
        if(cameraRig!=null)cameraRig.enabled=false;
        IsRiding=true;lookYaw=lookPitch=0;return true;
    }
    public bool TryDismount()
    {
        if(!IsRiding || SpeedKmh>9)return false;
        foreach(Vector3 offset in new[]{Vector3.left*1.3f,Vector3.right*1.3f,Vector3.back*1.8f,Vector3.forward*1.8f})
        {
            Vector3 at=transform.TransformPoint(offset);
            if(!GroundAt(at+Vector3.up*2.5f,5,out RaycastHit ground))continue;
            Vector3 feet=ground.point+Vector3.up*.04f;bool clear=true;
            foreach(var col in Physics.OverlapCapsule(feet+Vector3.up*.3f,feet+Vector3.up*1.5f,.29f,~0,QueryTriggerInteraction.Ignore))
                if(!col.transform.IsChildOf(player.transform)){clear=false;break;}
            if(!clear)continue;
            ReleasePlayer(feet+Vector3.up*.9f);return true;
        }
        return false;
    }
    void ReleasePlayer(Vector3 at)
    {
        IsRiding=false;throttle=steer=0;brake=true;
        player.transform.SetParent(previousParent,true);player.transform.position=at;player.GetComponent<CharacterController>().enabled=true;
        player.ResetVerticalSpeed();player.controlEnabled=true;
        foreach(var renderer in player.GetComponentsInChildren<Renderer>())renderer.enabled=true;
        if(cameraRig!=null){cameraRig.enabled=true;cameraRig.SetFollow(player.transform,ThirdPersonCamera.OnFootOffset,ThirdPersonCamera.OnFootDistance,true);cameraRig.Snap();}
    }
    public void ReleaseForRecovery()
    {
        if(!IsRiding)return;
        body.velocity=Vector3.zero;body.angularVelocity=Vector3.zero;
        ReleasePlayer(player.transform.position);
    }
    void LateUpdate()
    {
        if(!IsRiding || cameraRig==null)return;
        cameraRig.transform.SetPositionAndRotation(transform.TransformPoint(new Vector3(0,1.55f,.05f)),Quaternion.Euler(lookPitch,transform.eulerAngles.y+lookYaw,0));
        cameraRig.GetComponent<Camera>().fieldOfView=68;cameraRig.GetComponent<Camera>().nearClipPlane=.04f;
    }
    static GameObject Part(Transform parent,string name,Vector3 at,Vector3 size,Color color,PrimitiveType shape=PrimitiveType.Cube)
    {return LevelBuildUtil.Make(shape,parent,name,at,size,Quaternion.identity,color,"Bike_"+name,false);}
    public static CampusMotorcycle Create(Transform root,ThirdPersonController player,ThirdPersonCamera camera)
    {
        var go=new GameObject("GateMotorcycle");go.transform.SetParent(root,false);go.transform.position=new Vector3(-19.5f,.11f,59.5f);go.transform.rotation=Quaternion.Euler(0,38,0);
        var rigid=go.AddComponent<Rigidbody>();rigid.mass=220;
        var shell=go.AddComponent<BoxCollider>();shell.center=new Vector3(0,.65f,0);shell.size=new Vector3(.52f,.65f,1.55f);
        var bike=go.AddComponent<CampusMotorcycle>();bike.player=player;bike.cameraRig=camera;
        var visual=new GameObject("MotorcycleVisual").transform;visual.SetParent(go.transform,false);bike.visual=visual;
        Color black=new Color(.075f,.08f,.085f),metal=new Color(.5f,.52f,.53f),red=new Color(.53f,.085f,.06f);
        foreach(float z in new[]{-.7f,.7f})
        {
            var wheel=Part(visual,"Tyre",new Vector3(0,.32f,z),new Vector3(.64f,.075f,.64f),black,PrimitiveType.Cylinder).transform;
            wheel.localRotation=Quaternion.Euler(0,0,90);
            var rim=Part(visual,"AlloyRim",new Vector3(0,.32f,z),new Vector3(.43f,.079f,.43f),metal,PrimitiveType.Cylinder);rim.transform.localRotation=Quaternion.Euler(0,0,90);
            var physicsWheel=new GameObject("WheelContact");physicsWheel.transform.SetParent(go.transform,false);physicsWheel.transform.localPosition=new Vector3(0,.32f,z);
            physicsWheel.AddComponent<SphereCollider>().radius=.32f;
            if(z>0)bike.frontWheel=wheel;else bike.rearWheel=wheel;
        }
        Part(visual,"Engine",new Vector3(0,.48f,-.04f),new Vector3(.42f,.38f,.45f),metal);
        for(int i=0;i<5;i++)Part(visual,"EngineFin",new Vector3(0,.4f+i*.055f,.04f),new Vector3(.46f,.018f,.4f),black);
        var tank=Part(visual,"FuelTank",new Vector3(0,.89f,.16f),new Vector3(.49f,.32f,.64f),red,PrimitiveType.Sphere);tank.transform.localRotation=Quaternion.Euler(-12,0,0);
        Part(visual,"RiderSeat",new Vector3(0,.84f,-.4f),new Vector3(.4f,.13f,.65f),black);
        Part(visual,"RearBody",new Vector3(0,.74f,-.75f),new Vector3(.36f,.2f,.37f),red);
        foreach(float x in new[]{-.17f,.17f})
        {
            var fork=Part(visual,"FrontFork",new Vector3(x,.66f,.63f),new Vector3(.045f,.72f,.045f),metal);fork.transform.localRotation=Quaternion.Euler(-17,0,0);
            var frame=Part(visual,"FrameTube",new Vector3(x,.56f,-.22f),new Vector3(.045f,.055f,1.1f),black);frame.transform.localRotation=Quaternion.Euler(-20,0,0);
            Part(visual,"FootPeg",new Vector3(x*1.6f,.35f,-.2f),new Vector3(.2f,.055f,.08f),black);
        }
        Part(visual,"Handlebar",new Vector3(0,1.1f,.48f),new Vector3(.8f,.045f,.045f),metal);
        foreach(float x in new[]{-.36f,.36f})
        {Part(visual,"HandleGrip",new Vector3(x,1.1f,.48f),new Vector3(.15f,.065f,.07f),black);
         Part(visual,"MirrorStem",new Vector3(x,1.23f,.49f),new Vector3(.022f,.25f,.022f),metal);
         Part(visual,"Mirror",new Vector3(x,1.37f,.49f),new Vector3(.18f,.12f,.035f),GlassColor());}
        Part(visual,"Headlamp",new Vector3(0,.91f,.83f),new Vector3(.23f,.23f,.12f),new Color(.95f,.92f,.71f),PrimitiveType.Sphere);
        Part(visual,"TailLight",new Vector3(0,.78f,-.95f),new Vector3(.22f,.08f,.04f),Color.red);
        Part(visual,"Exhaust",new Vector3(.27f,.36f,-.57f),new Vector3(.12f,.12f,.65f),metal);
        return bike;
    }
    static Color GlassColor(){return new Color(.45f,.58f,.62f);}
}
