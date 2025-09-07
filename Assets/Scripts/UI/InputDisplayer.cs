using NaughtyAttributes;
using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;

public class InputDisplayer : MonoBehaviour
{
    private static readonly string KeyboardMouseInternalName = "Keyboard&Mouse";

    [Header("References")]
    [SerializeField] private InputActionReference inputAction;
    [SerializeField] private TMP_Text inputDisplayText;

    private void Start()
    {
        inputDisplayText.text = GetInputDisplayString(inputAction);
    }

    /// <summary>
    /// Gets the display string for the given input action based on the specified control scheme.
    /// </summary>
    public static string GetInputDisplayString(InputActionReference inputAction, InputBinding.DisplayStringOptions displayOptions = InputBinding.DisplayStringOptions.DontIncludeInteractions)
    {
        int bindingIndex = inputAction.action.bindings.IndexOf(binding => binding.groups.Contains($"{KeyboardMouseInternalName}"));
        return inputAction.action.GetBindingDisplayString(bindingIndex, displayOptions);
    }
}
