// Assets/Scripts/LSystemUIController.cs
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Controls the user interface for configuring and generating the L-System.
/// </summary>
public class LSystemUIController : MonoBehaviour
{
    #region Fields

    [Header("UI Elements")]
    [Tooltip("Dropdown to select different L-System variants.")]
    public Dropdown variantDropdown;

    [Tooltip("Slider to adjust the number of iterations.")]
    public Slider iterationSlider;

    [Tooltip("Input field to set the turning angle.")]
    public InputField angleInput;

    [Tooltip("Input field to set the branch length.")]
    public InputField lengthInput;

    [Tooltip("Button to generate and render the L-System.")]
    public Button generateButton;

    [Header("References")]
    [Tooltip("Reference to the LSystemRenderer component.")]
    public LSystemRenderer renderer;

    [Tooltip("Reference to the LSystemGenerator component.")]
    public LSystemGenerator generator;

    private List<LSystemConfig> configs;

    #endregion

    #region Unity Methods

    /// <summary>
    /// Initializes the UI by loading configurations, populating the dropdown, and setting up listeners.
    /// </summary>
    void Start()
    {
        LoadConfigs();
        PopulateVariantDropdown();
        AssignUIListeners();

        // Initialize default values from UI
        if (iterationSlider != null)
            generator.iterations = Mathf.RoundToInt(iterationSlider.value);

        if (angleInput != null && float.TryParse(angleInput.text, out float angle))
            generator.Angle = angle;
        else
            generator.Angle = 25f; // Default value

        if (lengthInput != null && float.TryParse(lengthInput.text, out float length))
            generator.Length = length;
        else
            generator.Length = 1f; // Default value
    }

    #endregion

    #region Private Methods

    /// <summary>
    /// Loads all L-System configurations from the Resources/LSystemConfigs folder.
    /// </summary>
    private void LoadConfigs()
    {
        configs = new List<LSystemConfig>(Resources.LoadAll<LSystemConfig>("LSystemConfigs"));
        if (configs.Count == 0)
        {
            Debug.LogWarning("No LSystemConfig assets found in Resources/LSystemConfigs.");
        }
    }

    /// <summary>
    /// Populates the variant dropdown with the loaded L-System configurations.
    /// </summary>
    private void PopulateVariantDropdown()
    {
        List<string> options = new List<string>();
        foreach (var config in configs)
        {
            options.Add(config.Name);
        }
        variantDropdown.ClearOptions();
        variantDropdown.AddOptions(options);
    }

    /// <summary>
    /// Assigns event listeners to the UI elements for interaction handling.
    /// </summary>
    private void AssignUIListeners()
    {
        if (generateButton != null)
            generateButton.onClick.AddListener(OnGenerateButtonClicked);

        if (iterationSlider != null)
            iterationSlider.onValueChanged.AddListener(OnIterationSliderChanged);

        if (angleInput != null)
            angleInput.onEndEdit.AddListener(OnAngleInputChanged);

        if (lengthInput != null)
            lengthInput.onEndEdit.AddListener(OnLengthInputChanged);
    }

    /// <summary>
    /// Handler for the Generate button click event. Configures the generator and triggers L-System generation.
    /// </summary>
    private void OnGenerateButtonClicked()
    {
        if (configs == null || configs.Count == 0)
        {
            Debug.LogError("No LSystemConfig assets loaded.");
            return;
        }

        int selectedVariant = variantDropdown.value;
        var config = configs[selectedVariant];

        // Update generator properties
        generator.axiom = config.Axiom;
        generator.rules = config.Rules;
        generator.iterations = Mathf.RoundToInt(iterationSlider.value);
        generator.Length = float.TryParse(lengthInput.text, out float length) ? length : config.Length;
        generator.Angle = float.TryParse(angleInput.text, out float angle) ? angle : config.Angle;

        // Generate L-System
        generator.GenerateLSystem();
    }

    /// <summary>
    /// Handler for changes in the iteration slider. Updates the generator's iteration count.
    /// </summary>
    /// <param name="value">The new slider value.</param>
    private void OnIterationSliderChanged(float value)
    {
        generator.iterations = Mathf.RoundToInt(value);
    }

    /// <summary>
    /// Handler for changes in the angle input field. Updates the generator's angle.
    /// </summary>
    /// <param name="value">The new angle value as a string.</param>
    private void OnAngleInputChanged(string value)
    {
        if (float.TryParse(value, out float angle))
        {
            generator.Angle = angle;
        }
        else
        {
            // Handle invalid input
            Debug.LogWarning("Invalid angle input. Using previous value.");
        }
    }

    /// <summary>
    /// Handler for changes in the length input field. Updates the generator's branch length.
    /// </summary>
    /// <param name="value">The new length value as a string.</param>
    private void OnLengthInputChanged(string value)
    {
        if (float.TryParse(value, out float length))
        {
            generator.Length = length;
        }
        else
        {
            // Handle invalid input
            Debug.LogWarning("Invalid length input. Using previous value.");
        }
    }

    #endregion
}