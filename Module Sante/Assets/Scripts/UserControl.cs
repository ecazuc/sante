using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UserControl : MonoBehaviour
{
    public float linearSpeed =5;
    public float angularSpeed = 180;
    private Rigidbody _rb;
    public float forceGain = 50000;
    private bool _isrbNotNull;

    // Start is called before the first frame update
    void Start()
    {
        Cursor.lockState = CursorLockMode.Confined;
        _rb = GetComponent<Rigidbody>();
    }

    // Update is called once per frame
    void Update()
    {
    }

    private void FixedUpdate(){
        if(_rb!=null){
            float dx = Mathf.Clamp(Input.GetAxis("Mouse X"),-1,1);
            //float dy = Mathf.Clamp(Input.GetAxis("Mouse Y"),-1,1);
            this.transform.Rotate(Vector3.up,angularSpeed*Time.deltaTime*dx,Space.World);
            //Vector3 moveDirection = Vector3.forward;
            //this.transform.Translate(moveDirection*Input.mouseScrollDelta.y*linearSpeed);
            //_rb.AddRelativeForce(moveDirection * (Time.fixedDeltaTime * forceGain));

            // click gauche pour un pas à gauche
            if (Input.GetMouseButton(0))
            {
                _rb.AddRelativeForce(Vector3.left*Time.fixedDeltaTime*forceGain);
            }

            // click droit pour un pas à droite
            if (Input.GetMouseButton(1))
            {
                _rb.AddRelativeForce(Vector3.right*Time.fixedDeltaTime*forceGain);
            }

            // click molette pour un pas en avant
            if (Input.GetMouseButton(2))
            {
                _rb.AddRelativeForce(Vector3.forward*Time.fixedDeltaTime*forceGain);
            }
            
            // molette vers le haut pour un pas en avant ou molette vers le bas pour un pas en arrière
            _rb.AddRelativeForce(Vector3.forward*Input.mouseScrollDelta.y*linearSpeed*Time.fixedDeltaTime*forceGain);
        }
    }
}
