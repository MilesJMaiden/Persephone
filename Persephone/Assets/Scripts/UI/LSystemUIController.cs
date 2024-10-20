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
        private TMP_InputField lengthInput;

        [SerializeField]
        private Button generateButton;

        [SerializeField]
        private Toggle isStochasticToggle;

        [SerializeField]
        private Toggle renderToggle;

        [SerializeField]
        private Toggle allNodesToggle;

        [SerializeField]
        private Toggle stochasticToggle;

        [SerializeField]
        private Slider rotationSlider;

        [SerializeField] // New Leaf Variant Dropdown
        private TMP_Dropdown leafVariantDropdown;


        [Header("L-System Configurations")]
        [SerializeField]
        private LSystemConfig[] lSystemConfigs;

        [SerializeField]
        private Toggle useMeshToggle;

        public event Action<LSystemConfig> OnGenerateRequested;
        public event Action<bool> OnRenderToggle;
        public event Action<bool> OnUseMeshToggleChanged;
        public event Action<int> OnLeafVariantSelected; // Event for Leaf Variant Selection

        private void Start()
        {
            InitializeDropdown();
            InitializeLeafVariantDropdown(); // Initialize leaf dropdown
            InitializeRotationSlider();

            iterationSlider.onValueChanged.AddListener(OnIterationSliderChanged);
            angleInput.onEndEdit.AddListener(OnAngleInputChanged);
            lengthInput.onEndEdit.AddListener(OnLengthInputChanged);
            generateButton.onClick.AddListener(OnGenerateButtonClicked);
            renderToggle.onValueChanged.AddListener(OnRenderToggleChanged);
            useMeshToggle.onValueChanged.AddListener(HandleUseMeshToggleChanged);
            allNodesToggle.onValueChanged.AddListener(OnAllNodesToggleChanged);

            stochasticToggle.onValueChanged.AddListener(OnStochasticToggleChanged);
            rotationSlider.onValueChanged.AddListener(OnRotationSliderChanged);
            leafVariantDropdown.onValueChanged.AddListener(OnLeafVariantDropdownChanged); // Listener for leaf variant dropdown
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

        private void InitializeLeafVariantDropdown()
        {
            leafVariantDropdown.ClearOptions();

            // Fetch the leafVariants array from the LSystemRenderer
            var lSystemRenderer = FindObjectOfType<LSystemRenderer>();

            if (lSystemRenderer != null && lSystemRenderer.leafVariants.Length > 0)
            {
                var leafOptions = new List<string>();

                // Add each prefab name to the dropdown options
                foreach (var leafVariant in lSystemRenderer.leafVariants)
                {
                    if (leafVariant != null)
                    {
                        leafOptions.Add(leafVariant.name); // Use the prefab's name property
                    }
                }

                leafVariantDropdown.AddOptions(leafOptions);
            }
            else
            {
                Debug.LogWarning("LSystemRenderer or leafVariants not set correctly.");
            }
        }


        private void OnLeafVariantDropdownChanged(int index) // Event for leaf variant change
        {
            OnLeafVariantSelected?.Invoke(index); // Invoke the event with the selected index
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
                variantDropdown.Hide();
                UpdateUIWithConfig(lSystemConfigs[index]);
            }

            if (inputModule != null)
            {
                StartCoroutine(ReenableInputModule(inputModule));
            }
        }

        private void OnStochasticToggleChanged(bool isOn)
        {
            // Assuming the LSystemRenderer is responsible for rendering
            var lSystemRenderer = FindObjectOfType<LSystemRenderer>();
            if (lSystemRenderer != null)
            {
                lSystemRenderer.SetStochasticMode(isOn);  // Example setter to update the mode
            }
        }

        private IEnumerator ReenableInputModule(InputSystemUIInputModule inputModule)
        {
            yield return new WaitForEndOfFrame();
            inputModule.enabled = true;
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
                angleInput.text = "25";
            }
        }

        private void OnLengthInputChanged(string value)
        {
            if (!float.TryParse(value, out float length))
            {
                Debug.LogWarning("Invalid length input. Using default value.");
                lengthInput.text = "1";
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
                    parsedAngle = 25f; // Default
                    angleInput.text = parsedAngle.ToString();
                }

                // Validate and parse length input
                if (!float.TryParse(lengthInput.text, out float parsedLength))
                {
                    Debug.LogWarning("Invalid length input. Using default value of 1.");
                    parsedLength = 1f; // Default value
                    lengthInput.text = parsedLength.ToString();
                }

                // Set the parsed values to the config
                config.DefaultIterations = Mathf.RoundToInt(iterationSlider.value);
                config.Angle = parsedAngle;
                config.Length = parsedLength;

                OnGenerateRequested?.Invoke(config);

                // Update the render call to include random offset
                var renderer = FindObjectOfType<LSystemRenderer>();
                if (renderer != null)
                {
                    renderer.Render(config.Axiom, parsedLength, parsedAngle);
                }
            }
            else
            {
                Debug.LogError("LSystemUIController: Selected config index is out of range.");
            }
        }

        private void OnRotationSliderChanged(float value)
        {
            var lSystemRenderer = FindObjectOfType<LSystemRenderer>();
            if (lSystemRenderer != null)
            {
                lSystemRenderer.RotateRenderer(value);
            }
        }

        private void InitializeRotationSlider()
        {
            var lSystemRenderer = FindObjectOfType<LSystemRenderer>();
            if (lSystemRenderer != null)
            {
                rotationSlider.value = lSystemRenderer.transform.rotation.eulerAngles.y;
            }
        }

        private void HandleUseMeshToggleChanged(bool isMeshOn)
        {
            Debug.Log($"Use Mesh Renderer Toggle Changed: {isMeshOn}");
            OnUseMeshToggleChanged?.Invoke(isMeshOn);
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
            iterationSlider.onValueChanged.RemoveListener(OnIterationSliderChanged);
            angleInput.onEndEdit.RemoveListener(OnAngleInputChanged);
            lengthInput.onEndEdit.RemoveListener(OnLengthInputChanged);
            generateButton.onClick.RemoveListener(OnGenerateButtonClicked);
            renderToggle.onValueChanged.RemoveListener(OnRenderToggleChanged);
            useMeshToggle.onValueChanged.RemoveListener(HandleUseMeshToggleChanged);
            allNodesToggle.onValueChanged.RemoveListener(OnAllNodesToggleChanged);
            stochasticToggle.onValueChanged.RemoveListener(OnStochasticToggleChanged);
            leafVariantDropdown.onValueChanged.RemoveListener(OnLeafVariantDropdownChanged); // Remove listener for leaf variant dropdown

            rotationSlider.onValueChanged.RemoveListener(OnRotationSliderChanged);
        }
    }
}
