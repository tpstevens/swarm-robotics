using UnityEngine;

using CommSystem;
using Utilities;

public class CommMessageBroadcast : MonoBehaviour
{
    private bool initialized = false;
    private bool justPassedThreshold = false;
    private CommMessage msg;

    public void FixedUpdate()
    {
        msg.distanceTraveled += Time.fixedDeltaTime * msg.propagationSpeed;

        Vector3 msgScale = transform.localScale;
        msgScale.x = msg.distanceTraveled * 2.0f;
        msgScale.z = msg.distanceTraveled * 2.0f;
        transform.localScale = msgScale;

        if (msg.distanceTraveled > msg.distanceLimit)
        {
            if (justPassedThreshold)
            {
                gameObject.SetActive(false);
                Comm.notifyDeletion(msg.id);
            }
            else
            {
                // Allow one more physics check
                justPassedThreshold = true;
            }
        }
    }

    public void initialize(CommMessage msg)
    {
        if (!initialized)
        {
            initialized = true;

            this.msg = msg;
        }
        else
        {
            Log.e(LogTag.COMM, "Attempted to initialize CommMessage " + msg.id + " twice");
        }
    }
    
    private void OnTriggerEnter(Collider collider)
    {
        Comm.notifyDelivery(msg.id, uint.Parse(collider.GetComponentInParent<Transform>().name.Substring(6)));
    }
}
