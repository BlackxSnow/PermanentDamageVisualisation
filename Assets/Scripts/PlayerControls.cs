using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerControls : MonoBehaviour
{
    class HitData
    {
        //Vectors should be in Object Space of HitCollider
        public Collider HitCollider;
        public Vector3 Position;
        public Vector3 PlaneNormal;
    }
    private HitData LastHit;

    public TMPro.TextMeshProUGUI StrengthText;

    public float Speed = 2;

    [Header("Weapon")]
    public float Size = 1;
    public float Falloff = 1;
    public float Strength = 1;

    // Start is called before the first frame update
    void Start()
    {
        Cursor.lockState = CursorLockMode.Locked;
        StrengthText.text = $"Strength: {Strength}";
    }

    // Update is called once per frame
    void Update()
    {
        if (Cursor.lockState == CursorLockMode.Locked)
        {
            Vector3 movement = new Vector3();
            movement.z = Input.GetAxis("Vertical");
            movement.x = Input.GetAxis("Horizontal");

            if (Input.GetKey(KeyCode.Space))
            {
                movement.y += 1;
            }
            if (Input.GetKey(KeyCode.LeftControl))
            {
                movement.y -= 1;
            }
            if (Input.GetKey(KeyCode.Mouse0))
            {
                Fire();
            }
            else
            {
                LastHit = null;
            }
            transform.Rotate(Vector3.up * Input.GetAxis("Mouse X"), Space.World);
            transform.Rotate(Vector3.right * -Input.GetAxis("Mouse Y"), Space.Self);

            transform.Translate(movement.normalized * Speed * Time.deltaTime, Space.Self);
        }
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            Cursor.lockState = Cursor.lockState == CursorLockMode.None ? CursorLockMode.Locked : CursorLockMode.None;
        }
        if(Input.mouseScrollDelta.y != 0)
        {
            Strength = Mathf.Round((Strength + Input.mouseScrollDelta.y / 10) * 10) / 10;
            
            StrengthText.text = $"Strength: {Strength}";
        }


    }

    private void Fire()
    {
        Ray ray = new Ray(transform.position, transform.forward);
        bool didRayHit = Physics.Raycast(ray, out RaycastHit hit);

        //On miss, raycast half way between the last hit and current direction for a partial line
        if(LastHit != null && (!didRayHit || hit.collider != LastHit.HitCollider))
        {
            //Secondary hit between LastHit and ray
            //hit = raycast
        }

        if (didRayHit)
        {
            Vector3 objSpacePos = hit.transform.worldToLocalMatrix.MultiplyPoint(hit.point);
            Vector3 objSpaceNormal = hit.transform.worldToLocalMatrix.MultiplyVector(hit.normal);

            //Register the line damage
            if (didRayHit && LastHit != null && hit.collider.TryGetComponent(out DamageBuffer buffer))
            {
                buffer.RegisterLineHit(
                    LastHit.Position,
                    objSpacePos,
                    Vector3.ProjectOnPlane(objSpacePos - LastHit.Position, LastHit.PlaneNormal),
                    Vector3.ProjectOnPlane(LastHit.Position - objSpacePos, objSpaceNormal),
                    Size,
                    Falloff,
                    Strength
                    );
            }

            //Set the LastHit to the current hit
            LastHit = new HitData
            {
                HitCollider = hit.collider,
                Position = objSpacePos,
                PlaneNormal = objSpaceNormal
            };
        }
        else
        {
            LastHit = null;
        }
    }
}
