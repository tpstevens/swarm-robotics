using UnityEngine;

using CommSystem;
using Utilities;

public class CommMessageBroadcast : MonoBehaviour
{
    private bool initialized = false;
    private bool justPassedThreshold = false;
    private CommMessage msg;
    private int numDeliveriesRemaining;

    public void Update()
    {
        msg.distanceTraveled += Time.deltaTime * msg.propagationSpeed;

        Vector3 msgScale = transform.localScale;
        msgScale.x = msg.distanceTraveled * 2.0f;
        msgScale.y = msg.distanceTraveled * 2.0f;
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
        else if (numDeliveriesRemaining == 0)
        {
            gameObject.SetActive(false);
            Comm.notifyDeletion(msg.id);
        }
    }

    public void initialize(CommMessage msg, int numRecipients)
    {
        if (!initialized)
        {
            initialized = true;

            this.msg = msg;
            numDeliveriesRemaining = numRecipients;
        }
        else
        {
            Log.e(LogTag.COMM, "Attempted to initialize CommMessage " + msg.id + " twice");
        }
    }
    
    private void OnTriggerEnter(Collider collider)
    {
        bool validReceiverId = true;
        Transform receiverTransform = collider.GetComponentInParent<Transform>();
        uint receiverId = uint.MaxValue;

        if (receiverTransform != null)
        {
            if (receiverTransform.CompareTag("Satellite"))
            {
                receiverId = Comm.SATELLITE;
            }
            else if (receiverTransform.CompareTag("Robot"))
            {
                receiverId = uint.Parse(receiverTransform.name.Substring(6));
            }
            else
            {
                Log.e(LogTag.COMM, "Collider parent transform doesn't match 'Robot' or 'Satellite'!");
                validReceiverId = false;
            }
        }
        else
        {
            Log.a(LogTag.COMM, "Collider parent missing transform!");
            validReceiverId = false;
        }

        if (validReceiverId && receiverId != msg.senderId)
        {
            Comm.notifyDelivery(msg.id, uint.Parse(collider.GetComponentInParent<Transform>().name.Substring(6)));
            --numDeliveriesRemaining;
        }
    }
}
