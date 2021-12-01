using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using UnityEngine.EventSystems;

public class TextBox : MonoBehaviour
{    
    [SerializeField, Tooltip("How long to type of all the words")] float typeTime = 1f;

    [SerializeField] GameObject textBoxGO;
    [SerializeField] Text message;
    [SerializeField] Button button;
    [SerializeField] Text buttonLabel;

    ButtonState buttonState;
    Queue<string> messageQueue;

    // Start is called before the first frame update
    void Start()
    {
        messageQueue = new Queue<string>();
        Close();
    }

    public void Close()
    {
        textBoxGO.SetActive(false);
        message.text = "";
        buttonLabel.text = "";
        EventSystem.current.SetSelectedGameObject(null);
    }

    public IEnumerator ShowMessageRoutine(List<string> messages)
    {
        // Show the message
        Open(messages);

        // Wait a bit to ensure the text box is activated
        yield return new WaitForEndOfFrame();

        // Wait until the text box is closed
        while (textBoxGO.activeSelf)
            yield return null;
    }

    public void Open(List<string> messages)
    {
        // Nothing to do
        if (messages.Count < 1)
            return;

        messageQueue = new Queue<string>(messages);
        StartCoroutine(OpenTextBoxRoutine());
    }

    IEnumerator OpenTextBoxRoutine()
    {
        // Clear the message
        message.text = "";
        EventSystem.current.SetSelectedGameObject(null);

        // Shrink the box and enable it since others depend on 
        // this being active to know when the routine is done
        textBoxGO.gameObject.transform.localScale = Vector3.zero;
        textBoxGO.SetActive(true);

        // Hide the button while scaling so it does not look weird
        SetButtonState(ButtonState.Hidden);

        // Open text box
        textBoxGO.gameObject.transform.localScale = new Vector3(1f, 1f, 1f);

        // Type out the text
        var text = messageQueue.Dequeue();

        // State based on how many messages we are showing
        var state = ButtonState.Close;
        if (messageQueue.Count > 0)
            state = ButtonState.Next;

        yield return StartCoroutine(ShowText(text, state));

        SetButtonState(state);
    }

    IEnumerator ShowText(string text, ButtonState state)
    {
        // Hide the button while the text reveals itself
        SetButtonState(ButtonState.Hidden);

        // Clear existing text
        message.text = "";

        var chars = text.ToCharArray();
        var delay = typeTime / chars.GetLength(0);
        foreach (var c in chars)
        {
            message.text += c;
            // AudioManager.instance.PlayClip(SFXLibrary.instance.typeTextClip);
            yield return new WaitForSeconds(delay);
        }

        // Now set the button to whatever it needs to be
        SetButtonState(state);
    }

    void SetButtonState(ButtonState state)
    {
        buttonState = state;        
        buttonLabel.text = state.ToString().ToUpper();

        // Show the button last to have the label updated first
        button.gameObject.SetActive(state != ButtonState.Hidden);

        EventSystem.current.SetSelectedGameObject(button.gameObject);
    }

    public void OnButtonClicked()
    {
        switch (buttonState)
        {
            case ButtonState.Next:
                var text = messageQueue.Dequeue();
                var state = ButtonState.Next;
                if (messageQueue.Count < 1)
                    state = ButtonState.Close;

                StartCoroutine(ShowText(text, state));                
                break;

            case ButtonState.Close:
                Close();
                break;
        }
    }
}
