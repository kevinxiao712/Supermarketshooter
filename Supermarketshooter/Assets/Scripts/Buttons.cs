using UnityEngine;

public class Buttons : MonoBehaviour
{

    public GameObject objectToDisable;

    // Called by the "Quit" button
    public void QuitApplication()
    {

        Application.Quit();

    }

    // lmao so scuffed
    public void DisableReferencedObject()
    {
        if (objectToDisable != null)
        {
            objectToDisable.SetActive(false);
        }
    }
}

