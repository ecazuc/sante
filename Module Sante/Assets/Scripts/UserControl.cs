using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UserControl : MonoBehaviour
{
    public float linearSpeed = 1;
    public float angularSpeed = 90;
    private Rigidbody _rb;
    public float forceGain = 100000;
    private bool _isrbNotNull;

    // Start is called before the first frame update
    void Start()
    {
        _isrbNotNull = _rb!=null;
        Cursor.lockState = CursorLockMode.Confined;
        _rb = GetComponent<Rigidbody>();
    }

    // Update is called once per frame
    void Update()
    {
    }

    private void FixedUpdate(){
        if(_isrbNotNull){
            float dx = Mathf.Clamp(Input.GetAxis("Mouse X"),-1,1);
            float dy = Mathf.Clamp(Input.GetAxis("Mouse Y"),-1,1);
            this.transform.Rotate(Vector3.up,angularSpeed*Time.deltaTime*dx,Space.World);
            Vector3 moveDirection = Vector3.forward*dy;
            _rb.AddRelativeForce(moveDirection * (Time.fixedDeltaTime * forceGain));
        }
    }
}
