using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using hapticDriver;
using UnityEditor;
using UnityEngine;
using System;

public class VibrationManager_minimal: MonoBehaviour
{
    public string devicePort = "COM4"; // Check which port is used on your system !
    private Timer callbackTimer;
    private Driver driver;
    
    
    // Examples of values that can be edited in the inspector
    public int vibrateTime_ms = 0;
    
    [Range(0,255)]
    public float vibrateIntensity = 0.0f;

    public bool activeMotor1;
    public bool activeMotor2;
    public bool activeMotor3;
    public bool activeMotor4;
    
    
    public List<Sequence> vibrationSequence = new List<Sequence>{}; // The elements type must be Serializable   
    
    // the Start function is called when a script is enabled
    private void Start()
    {
        // create a new driver instance
        driver = new Driver(devicePort);
        // create a new timer that will call the emitterCallback function every 40ms
        callbackTimer = new Timer(emitterCallback, null, 0, 40);
    }
    
    // this function is called every 40ms
    private void emitterCallback(object state)
    {
        //driver.SetMessage(getDefaultMessage());
        driver.SendMessage();
    }
    
    // default message is a message with all motors at 0 + the end marker
    private byte[] getDefaultMessage()
    {
        return new byte[5] { 0, 0, 0, 0, Driver.EndMarker };
    }
    
    // example of a function that will play a vibration on one motor
    // this function is asynchronous, meaning that it will not block the main thread
    public async void playOne(int buttonId)
    {
        byte[] message = getDefaultMessage();
        message[buttonId] = (byte)vibrateIntensity;
        driver.SetMessage(message);
        await Task.Delay(vibrateTime_ms); // in ms, but Unity Time.time is in s
        driver.SetMessage(getDefaultMessage());
    }
    
    // example of a function that will play a vibration on all the motors
    // this function is asynchronous, meaning that it will not block the main thread
    public async void playAll()
    {
        driver.SetMessage(new byte[5] { (byte) vibrateIntensity, (byte)vibrateIntensity,(byte) vibrateIntensity, (byte)vibrateIntensity, Driver.EndMarker });
        await Task.Delay(vibrateTime_ms); // in ms, but Unity Time.time is in s
        driver.SetMessage(getDefaultMessage());
    }
    
    // example of a function that will play a vibration on the motors that are checked
    // this function is asynchronous, meaning that it will not block the main thread
    public async void playSimultaneous()
    {
        driver.SetMessage(new byte[5] { 
            activeMotor1? (byte) vibrateIntensity: (byte)0, 
            activeMotor2? (byte) vibrateIntensity: (byte)0,
            activeMotor3? (byte) vibrateIntensity: (byte)0,
            activeMotor4? (byte) vibrateIntensity: (byte)0,
            Driver.EndMarker });
        await Task.Delay(vibrateTime_ms); // in ms, but Unity Time.time is in s
        driver.SetMessage(getDefaultMessage());
    }  
    
    public async void startSequence(){
        for(int i=0;i<vibrationSequence.Count;i++){
            playSequence(vibrationSequence[i]);
            await Task.Delay(vibrateTime_ms); // in ms, but Unity Time.time is in s
        }
        driver.SetMessage(getDefaultMessage());
    }
    
    // example of a function that will play a vibration sequence on the motors
    // this function is asynchronous, meaning that it will not block the main thread
    public async void playSequence(Sequence sequence)
    {
        driver.SetMessage(new byte[5] { 
            (byte) sequence.intensityMotor1, 
            (byte) sequence.intensityMotor2,
            (byte) sequence.intensityMotor3,
            (byte) sequence.intensityMotor4,
            Driver.EndMarker });
        await Task.Delay(vibrateTime_ms); // in ms, but Unity Time.time is in s
    }

    public void avance()
    {
        vibrationSequence = new List<Sequence>();
        vibrationSequence.Add(new Sequence(0, 140, 140, 0));
        vibrateTime_ms = 200;
        startSequence();
    }
    
    public void droite()
    {
        vibrationSequence = new List<Sequence>();
        vibrationSequence.Add(new Sequence(0, 0, 140, 140));
        vibrateTime_ms = 200;
        startSequence();
    }
    
    public void gauche()
    {
        vibrationSequence = new List<Sequence>();
        vibrationSequence.Add(new Sequence(140, 140, 0, 0));
        vibrateTime_ms = 200;
        startSequence();
    }
    
    public void recule()
    {
        vibrationSequence = new List<Sequence>();
        vibrationSequence.Add(new Sequence(140, 0, 0, 140));
        vibrationSequence.Add(new Sequence(0, 140, 140, 0));
        vibrationSequence.Add(new Sequence(140, 0, 0, 140));
        vibrationSequence.Add(new Sequence(0, 140, 140, 0));
        vibrateTime_ms = 200;
        startSequence();
    }

    public void obstacleGauche()
    {
        vibrationSequence = new List<Sequence>();
        vibrationSequence.Add(new Sequence(140, 0, 0, 0));
        vibrateTime_ms = 200;
        startSequence();
    }

    public void obstacleDroite()
    {
        vibrationSequence = new List<Sequence>();
        vibrationSequence.Add(new Sequence(0, 0, 0, 140));
        vibrateTime_ms = 200;
        startSequence();
    }

    public void tourne_droite()
    {
        vibrationSequence = new List<Sequence>();
        vibrationSequence.Add(new Sequence(0, 0, 140, 0));
        vibrationSequence.Add(new Sequence(0, 0, 0, 140));
        vibrateTime_ms = 200;
        startSequence();
    }
    public void tourne_gauche()
    {
        vibrationSequence = new List<Sequence>();
        vibrationSequence.Add(new Sequence(0, 140, 0, 0));
        vibrationSequence.Add(new Sequence(140, 0, 0, 0));
        vibrateTime_ms = 200;
        startSequence();
    }
    public void succes()
    {
        vibrationSequence = new List<Sequence>();
        vibrationSequence.Add(new Sequence(140, 0, 0, 0));
        vibrationSequence.Add(new Sequence(0, 140, 0, 0));
        vibrationSequence.Add(new Sequence(0, 0, 140, 0));
        vibrationSequence.Add(new Sequence(0, 0, 0, 140));
        vibrationSequence.Add(new Sequence(0, 0, 0, 140));
        vibrationSequence.Add(new Sequence(0, 0, 140, 0));
        vibrationSequence.Add(new Sequence(0, 140, 0, 0));
        vibrationSequence.Add(new Sequence(140, 0, 0, 0));
        vibrateTime_ms = 100;
        startSequence();
    }
    
    public void echec()
    {
        vibrationSequence = new List<Sequence>();
        vibrationSequence.Add(new Sequence(140, 140, 140, 140));
        vibrateTime_ms = 200;
        startSequence();
    }
    
    
    
    
}

[Serializable]
public class Sequence{
    [Range(0,255)]
	public int intensityMotor1;
    [Range(0,255)]
	public int intensityMotor2;
    [Range(0,255)]
	public int intensityMotor3;
    [Range(0,255)]
	public int intensityMotor4;

	public Sequence(int i1, int i2, int i3, int i4){
		intensityMotor1 = i1;
		intensityMotor2 = i2;
		intensityMotor3 = i3;
		intensityMotor4 = i4;
	}
}


// this is the editor script that will be used to display buttons in the inspector
[CustomEditor(typeof(VibrationManager_minimal))]
public class VibrationManagerEditor : Editor
{
    // instance is the object that is being edited/displayed
    private VibrationManager_minimal instance;
    private void OnEnable()
    {
        instance = (VibrationManager_minimal)target;
    }

    // this function is called when the inspector is drawn
    // this is where we can add buttons
    public override void OnInspectorGUI()
    {
        base.OnInspectorGUI(); // draw the default inspector

        // Button example
        if (GUILayout.Button("Vibrate motor 1")) instance.playOne(0);

        // Button example
        if (GUILayout.Button("Vibrate motor 2")) instance.playOne(1);

        // Button example
        if (GUILayout.Button("Vibrate motor 3")) instance.playOne(2);

        // Button example
        if (GUILayout.Button("Vibrate motor 4")) instance.playOne(3);

        // Button example
        if (GUILayout.Button("Vibrate all")) instance.playAll();

        // Button example
        if (GUILayout.Button("Vibrate simultaneous")) instance.playSimultaneous();

        // Button example
        if (GUILayout.Button("Vibrate sequence")) instance.startSequence();
        
        //Button avance
        if (GUILayout.Button("Avance")) instance.avance();
        
        //Button droite
        if (GUILayout.Button("Droite")) instance.droite();
        
        //Button gauche
        if (GUILayout.Button("Gauche")) instance.gauche();
        
        //Button recule
        if (GUILayout.Button("Recule")) instance.recule();
        
        //Button obstacle gauche
        if (GUILayout.Button("Obstacle gauche")) instance.obstacleGauche();
        
        //Button obstacle droite
        if (GUILayout.Button("Obstacle droite")) instance.obstacleDroite();
        
        //Button tourne droite
        if (GUILayout.Button("Tourne droite")) instance.tourne_droite();
        
        //Button tourne gauche
        if (GUILayout.Button("Tourne Gauche")) instance.tourne_gauche();
        
        //Button succes
        if (GUILayout.Button("Succès")) instance.succes();
        
        //Button echec
        if (GUILayout.Button("Echec")) instance.echec();
    }
}