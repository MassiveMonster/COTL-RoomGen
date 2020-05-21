using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraFollowTarget : MonoBehaviour {
    public Transform target;
    public List<GameObject> targets = new List<GameObject>();
    public Vector3 targetPosition = Vector3.zero;
    public float targetDistance = 10;
    public float distance = 10;
    public float MaxDistance = 11;
    bool init = false;
    [HideInInspector]
    public float angle = -45;
    public Vector3 TargetOffset = Vector3.zero;
    Vector3 CurrentOffset = Vector3.zero;
    [HideInInspector]
    public bool IN_CONVERSATION = false;
    public Vector3 Look;
    Vector3 CamWobble;
    float Wobble;
    float LookSpeed = 5;
    float LookDistance = 1;
    public bool DisablePlayerLook = false;

    public void SnapTo(Vector3 position)
    {
        transform.position =  position + new Vector3(0, distance * Mathf.Sin(Mathf.Deg2Rad * angle), -distance * Mathf.Cos(Mathf.Deg2Rad * angle));
    }

    public void SetOffset(Vector3 Offset)
    {
        TargetOffset = Offset;
    }

    void Update()
    {
        
        if (!init)
        {
            if (targets.Count <= 0)
            {
              
                if (targets.Count <= 0) return;
            }
            else
            {

            }
          

            init = true;
        }
        else
        {
            if (targets != null && targets.Count > 0)
            {
                for (int i = 0; i < targets.Count; i++)
                {
                    if (targets[i] == null)
                        targets.Remove(targets[i]);
                }


                
                    Vector3 Left = new Vector3(int.MaxValue,0);
                    Vector3 Right = new Vector3(-int.MaxValue, 0);
                    Vector3 Top = new Vector3(0,-int.MaxValue);
                    Vector3 Bottom = new Vector3(0, int.MaxValue);
                    Vector3 Up = new Vector3(0, -int.MaxValue);
                    Vector3 Down = new Vector3(0, int.MaxValue);
                  foreach (GameObject t in targets)
                  {
                        if (t != null)
                        {
                            if (t.transform.position.x < Left.x)
                                Left = t.transform.position;

                            if (t.transform.position.x > Right.x)
                                Right = t.transform.position;

                            if (t.transform.position.y > Top.y)
                                Top = t.transform.position;

                            if (t.transform.position.y < Bottom.y)
                                Bottom = t.transform.position;

                           

                        }
                  }
             

                targetPosition = new Vector3(Left.x + (Right.x - Left.x) / 2, Bottom.y + ((Top.y - Bottom.y) / 2), Down.z + ((Up.z - Down.z) / 2) - 0.5f);// targets[0].transform.position.z);

                if (targets.Count == 1 )
                {
                        Look = Vector3.Lerp(Look, Vector3.zero, Time.deltaTime * LookSpeed);
                }
                else Look = Vector3.zero;

                if (!IN_CONVERSATION)
                {
                    if (Camera.main.WorldToScreenPoint(targetPosition).x - Camera.main.WorldToScreenPoint(Left).x > 350)
                        targetDistance += 0.1f;

                    if (Camera.main.WorldToScreenPoint(targetPosition).y - Camera.main.WorldToScreenPoint(Bottom).y > 150)
                        targetDistance += 0.1f;

                    if (Camera.main.WorldToScreenPoint(targetPosition).x - Camera.main.WorldToScreenPoint(Left).x < 300 && targetDistance > MaxDistance && Camera.main.WorldToScreenPoint(targetPosition).y - Camera.main.WorldToScreenPoint(Bottom).y < 100)
                    {
                        targetDistance--;
                        if (targetDistance < MaxDistance) targetDistance = MaxDistance;
                    }

                    distance += (targetDistance - distance) / 3;
                    CamWobble = Vector3.zero;
                }
                else
                {
                    distance += (targetDistance - distance) / 7;
                    CamWobble += Vector3.forward * 0.003f * Mathf.Cos(Wobble += Time.deltaTime * 2);

                }


                transform.position = Vector3.Lerp(transform.position, targetPosition + new Vector3(0, distance * Mathf.Sin(Mathf.Deg2Rad * angle), -distance * Mathf.Cos(Mathf.Deg2Rad * angle)) + CamWobble + Look + (CurrentOffset = Vector3.Lerp(CurrentOffset, TargetOffset, 5 * Time.deltaTime)), 5 * Time.deltaTime) ;
            }
            else
            {
                if (target != null)
                {
                    targetPosition = target.position;
                    distance += (targetDistance - distance) / 7;
                    transform.position = Vector3.Lerp(transform.position, targetPosition + new Vector3(0, distance * Mathf.Sin(Mathf.Deg2Rad * angle), -distance * Mathf.Cos(Mathf.Deg2Rad * angle)), 5 * Time.deltaTime);
                }
                else
                {
                    distance += ((10 - distance) / 7) * (Time.deltaTime * 60);
                    transform.position = Vector3.Lerp(transform.position, targetPosition + new Vector3(0, distance * Mathf.Sin(Mathf.Deg2Rad * angle), -distance * Mathf.Cos(Mathf.Deg2Rad * angle)), 0.5f * Time.deltaTime);
                }
            }

           
        }


    }

   

}
