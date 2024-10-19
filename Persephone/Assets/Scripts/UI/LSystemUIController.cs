using UnityEngine;
using TMPro;
using System;
using ProceduralGraphics.LSystems.ScriptableObjects;
using System.Collections.Generic;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem.UI;
using System.Collections;
using ProceduralGraphics.LSystems.Rendering;

namespace ProceduralGraphics.LSystems.UI
{
    public class LSystemUIController : MonoBehaviour
    {
        [Header("UI Elements")]
        [SerializeField]
        private TMP_Dropdown variantDropdown;

        [SerializeField]
        private Slider iterationSlider;

        [SerializeField]
        private TMP_Text iterationLabel;

        [SerializeField]
        private TMP_InputField angleInput;

        [SerializeField]
        private TMP_InputField randomOffsetInput;

        [SerializeField]
        private TMP_InputField lengthInput;

        [SerializeField]
        private Button generateButton;

        [SerializeField]
        private Toggle renderToggle;

        [SerializeField]
        private Toggle allNodesToggle; // New toggle for activating/deactivating all nodes

        [Header("L-System Configurations")]
        [SerializeField]
        private LSystemConfig[] lSystemConfigs;

        [SerializeField]
        private Toggle is3DToggle;

        [SerializeField]
        private Toggle useMeshToggle;

        // Events for generating, rendering, toggling 3D, and mesh renderer
        public event Action<LSystemConfig> OnGenerateRequested;
        public event Action<bool> OnRenderToggle;
        public event Action<bool> On3DToggleChanged;
        public event Action<bool> OnUseMeshToggleChanged;

        private void Start()
        {
            InitializeDropdown();
            iterationSlider.onValueChanged.AddListener(OnIterationSliderChanged);
            angleInput.onEndEdit.AddListener(OnAngleInputChanged);
            randomOffsetInput.onEndEdit.AddListener(OnRandomOffsetInputChanged);
            lengthInput.onEndEdit.AddListener(OnLengthInputChanged);
            generateButton.onClick.AddListener(OnGenerateButtonClicked);
            renderToggle.onValueChanged.AddListener(OnRenderToggleChanged);
            is3DToggle.onValueChanged.AddListener(OnIs3DToggleChanged);
            useMeshToggle.onValueChanged.AddListener(HandleUseMeshToggleChanged);
            allNodesToggle.onValueChanged.AddListener(OnAllNodesToggleChanged); // Add listener for new toggle
        }

        private void InitializeDropdown()
        {
            variantDropdown.ClearOptions();
            var options = new List<string>();
            foreach (var config in lSystemConfigs)
            {
                options.Add(config.Name);
            }
            variantDropdown.AddOptions(options);
            variantDropdown.onValueChanged.AddListener(OnVariantDropdownChanged);
        }

        private void OnAllNodesToggleChanged(bool isOn)
        {
            // Directly call a method in the LSystemRenderer to manage node visibility
            var lSystemRenderer = FindObjectOfType<LSystemRenderer>();
            if (lSystemRenderer != null)
            {
                lSystemRenderer.SetAllNodesActive(isOn);
            }
        }

        private void OnVariantDropdownChanged(int index)
        {
            // Temporarily disable InputSystemUIInputModule to prevent processing destroyed object events
            var inputModule = EventSystem.current.GetComponent<InputSystemUIInputModule>();
            if (inputModule != null)
            {
                inputModule.enabled = false;
            }

            if (variantDropdown != null && index >= 0 && index < lSystemConfigs.Length)
            {
                variantDropdown.Hide(); // Ensure the dropdown is hidden before continuing
                UpdateUIWithConfig(lSystemConfigs[index]);
            }

            // Re-enable the Input System UI Input Module after dropdown changes
            if (inputModule != null)
            {
                StartCoroutine(ReenableInputModule(inputModule));
            }
        }

        // Coroutine to safely re-enable the Input System after dropdown changes
        private IEnumerator ReenableInputModule(InputSystemUIInputModule inputModule)
        {
            yield return new WaitForEndOfFrame();  // Wait until end of frame
            inputModule.enabled = true;  // Re-enable the input system module
        }

        private void UpdateUIWithConfig(LSystemConfig config)
        {
            if (config == null) return;

            iterationSlider.value = config.DefaultIterations;
            iterationLabel.text = $"Iterations: {config.DefaultIterations}";
            angleInput.text = config.Angle.ToString();
            lengthInput.text = config.Length.ToString();
        }

        private void OnIterationSliderChanged(float value)
        {
            int iterations = Mathf.RoundToInt(value);
            iterationLabel.text = $"Iterations: {iterations}";
        }

        private void OnAngleInputChanged(string value)
        {
            if (!float.TryParse(value, out float angle))
            {
                Debug.LogWarning("Invalid angle input. Using default value.");
                angleInput.text = "25"; // Default value
            }
        }

        // Add this method to handle random offset input
        private void OnRandomOffsetInputChanged(string value)
        {
            if (!float.TryParse(value, out float offset))
            {
                Debug.LogWarning("Invalid random offset input. Using default value of 0.");
                randomOffsetInput.text = "0";
            }
        }

        private void OnLengthInputChanged(string value)
        {
            if (!float.TryParse(value, out float length))
            {
                Debug.LogWarning("Invalid length input. Using default value.");
                lengthInput.text = "1"; // Default value
            }
        }

        private void OnGenerateButtonClicked()
        {
            int selectedIndex = variantDropdown.value;
            if (selectedIndex >= 0 && selectedIndex < lSystemConfigs.Length)
            {
                LSystemConfig config = lSystemConfigs[selectedIndex];

                // Validate and parse angle input
                if (!float.TryParse(angleInput.text, out float parsedAngle))
                {
                    Debug.LogWarning("Invalid angle input. Using default value of 25.");
                    parsedAngle = 25f; // Default value
                    angleInput.text = parsedAngle.ToString(); // Set the default in the UI as well
                }

                // Validate and parse length input
                if (!float.TryParse(lengthInput.text, out float parsedLength))
                {
                    Debug.LogWarning("Invalid length input. Using default value of 1.");
                    parsedLength = 1f; // Default value
                    lengthInput.text = parsedLength.ToString(); // Set the default in the UI as well
                }

                // Parse random offset input
                if (!float.TryParse(randomOffsetInput.text, out float randomOffset))
                {
                    Debug.LogWarning("Invalid random offset input. Using default value of 0.");
                    randomOffset = 0f; // Default value
                    randomOffsetInput.text = randomOffset.ToString(); // Set the default in the UI as well
                }

                // Set the parsed values to the config
                config.DefaultIterations = Mathf.RoundToInt(iterationSlider.value);
                config.Angle = parsedAngle;
                config.Length = parsedLength;
                config.RandomOffset = randomOffset; // Set the random offset

                // Trigger the event to generate the plant
                OnGenerateRequested?.Invoke(config);

                // Update the render call to include random offset
                var renderer = FindObjectOfType<LSystemRenderer>();
                if (renderer != null)
                {
                    renderer.Render(config.Axiom, parsedLength, parsedAngle, randomOffset);
                }
            }
            else
            {
                Debug.LogError("LSystemUIController: Selected config index is out of range.");
            }
        }


        private void OnIs3DToggleChanged(bool is3D)
        {
            Debug.Log($"Is3D Toggle Changed: {is3D}");
            On3DToggleChanged?.Invoke(is3D);
        }

        private void HandleUseMeshToggleChanged(bool isMeshOn)
        {
            Debug.Log($"Use Mesh Renderer Toggle Changed: {isMeshOn}");
            OnUseMeshToggleChanged?.Invoke(isMeshOn);  // Proper event invocation
        }

        private void OnRenderToggleChanged(bool isOn)
        {
            OnRenderToggle?.Invoke(isOn);
        }

        public void RegeneratePlant()
        {
            OnGenerateButtonClicked();
        }

        private void OnDestroy()
        {
            // Remove listeners to prevent memory leaks
            iterationSlider.onValueChanged.RemoveListener(OnIterationSliderChanged);
            angleInput.onEndEdit.RemoveListener(OnAngleInputChanged);
            lengthInput.onEndEdit.RemoveListener(OnLengthInputChanged);
            generateButton.onClick.RemoveListener(OnGenerateButtonClicked);
            renderToggle.onValueChanged.RemoveListener(OnRenderToggleChanged);
            is3DToggle.onValueChanged.RemoveListener(OnIs3DToggleChanged);
            useMeshToggle.onValueChanged.RemoveListener(HandleUseMeshToggleChanged);
            allNodesToggle.onValueChanged.RemoveListener(OnAllNodesToggleChanged); // Remove listener for the new toggle
        }
    }
}
