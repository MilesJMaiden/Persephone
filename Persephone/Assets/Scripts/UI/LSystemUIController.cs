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

        [SerializeField]
        private TMP_Dropdown leafVariantDropdown;

        // New UI controls for custom parameters
        [SerializeField]
        private TMP_InputField lengthVariationInput;

        [SerializeField]
        private TMP_InputField thicknessVariationInput;

        [SerializeField]
        private TMP_InputField curvatureAngleInput;

        [SerializeField]
        private TMP_InputField leafPlacementProbabilityInput;

        [Header("L-System Configurations")]
        [SerializeField]
        private LSystemConfig[] lSystemConfigs;

        [SerializeField]
        private Toggle useMeshToggle;

        public event Action<LSystemConfig> OnGenerateRequested;
        public event Action<bool> OnRenderToggle;
        public event Action<bool> OnUseMeshToggleChanged;
        public event Action<int> OnLeafVariantSelected;

        private void Start()
        {
            InitializeDropdown();
            InitializeLeafVariantDropdown();
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
            leafVariantDropdown.onValueChanged.AddListener(OnLeafVariantDropdownChanged);

            lengthVariationInput.onEndEdit.AddListener(OnLengthVariationChanged);
            thicknessVariationInput.onEndEdit.AddListener(OnThicknessVariationChanged);
            curvatureAngleInput.onEndEdit.AddListener(OnCurvatureAngleChanged);
            leafPlacementProbabilityInput.onEndEdit.AddListener(OnLeafPlacementProbabilityChanged);

            // Disable the rotation slider initially
            rotationSlider.interactable = false;
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

            var lSystemRenderer = FindObjectOfType<LSystemRenderer>();
            if (lSystemRenderer != null && lSystemRenderer.leafVariants.Length > 0)
            {
                var leafOptions = new List<string>();
                foreach (var leafVariant in lSystemRenderer.leafVariants)
                {
                    if (leafVariant != null)
                    {
                        leafOptions.Add(leafVariant.name);
                    }
                }
                leafVariantDropdown.AddOptions(leafOptions);
            }
            else
            {
                Debug.LogWarning("LSystemRenderer or leafVariants not set correctly.");
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

        private IEnumerator EnableRotationSliderAfterGeneration(LSystemRenderer renderer)
        {
            // Wait until the renderer is done with its coroutine
            yield return new WaitUntil(() => renderer.IsRenderingComplete);

            // Enable the rotation slider
            rotationSlider.interactable = true;
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

        private void HandleUseMeshToggleChanged(bool isMeshOn)
        {
            Debug.Log($"Use Mesh Renderer Toggle Changed: {isMeshOn}");
            OnUseMeshToggleChanged?.Invoke(isMeshOn);
        }

        private void OnRenderToggleChanged(bool isOn)
        {
            OnRenderToggle?.Invoke(isOn);
        }

        private void OnAllNodesToggleChanged(bool isOn)
        {
            var lSystemRenderer = FindObjectOfType<LSystemRenderer>();
            if (lSystemRenderer != null)
            {
                lSystemRenderer.SetAllNodesActive(isOn);
            }
        }

        private void OnStochasticToggleChanged(bool isOn)
        {
            var lSystemRenderer = FindObjectOfType<LSystemRenderer>();
            if (lSystemRenderer != null)
            {
                lSystemRenderer.SetStochasticMode(isOn);
            }
        }

        private void OnRotationSliderChanged(float value)
        {
            var lSystemRenderer = FindObjectOfType<LSystemRenderer>();
            if (lSystemRenderer != null && lSystemRenderer.IsRenderingComplete) // Check rendering status
            {
                lSystemRenderer.RotateRenderer(value);
            }
            else
            {
                rotationSlider.value = 0; // Reset slider if rendering is not complete
            }
        }


        private void OnLeafVariantDropdownChanged(int index)
        {
            OnLeafVariantSelected?.Invoke(index);
        }

        private void OnVariantDropdownChanged(int index)
        {
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

            lengthVariationInput.text = config.LengthVariationFactor.ToString();
            thicknessVariationInput.text = config.ThicknessVariationFactor.ToString();
            curvatureAngleInput.text = config.CurvatureAngle.ToString();
            leafPlacementProbabilityInput.text = config.LeafPlacementProbability.ToString();
        }

        private void OnLengthVariationChanged(string value)
        {
            if (!float.TryParse(value, out float variation))
            {
                Debug.LogWarning("Invalid length variation input.");
                return;
            }
            lSystemConfigs[variantDropdown.value].LengthVariationFactor = variation;
        }

        private void OnThicknessVariationChanged(string value)
        {
            if (!float.TryParse(value, out float thickness))
            {
                Debug.LogWarning("Invalid thickness variation input.");
                return;
            }
            lSystemConfigs[variantDropdown.value].ThicknessVariationFactor = thickness;
        }

        private void OnCurvatureAngleChanged(string value)
        {
            if (!float.TryParse(value, out float angle))
            {
                Debug.LogWarning("Invalid curvature angle input.");
                return;
            }
            lSystemConfigs[variantDropdown.value].CurvatureAngle = angle;
        }

        private void OnLeafPlacementProbabilityChanged(string value)
        {
            if (!float.TryParse(value, out float probability))
            {
                Debug.LogWarning("Invalid leaf placement probability input.");
                return;
            }
            lSystemConfigs[variantDropdown.value].LeafPlacementProbability = Mathf.Clamp01(probability);
        }

        private void OnGenerateButtonClicked()
        {
            // Disable the generate button and rotation slider at the start of generation
            generateButton.interactable = false;
            rotationSlider.interactable = false;

            int selectedIndex = variantDropdown.value;

            if (selectedIndex >= 0 && selectedIndex < lSystemConfigs.Length)
            {
                LSystemConfig config = lSystemConfigs[selectedIndex];

                // Validate and parse angle input
                if (!float.TryParse(angleInput.text, out float parsedAngle))
                {
                    Debug.LogWarning("Invalid angle input. Using default value of 25.");
                    parsedAngle = 25f;
                    angleInput.text = parsedAngle.ToString();
                }

                // Validate and parse length input
                if (!float.TryParse(lengthInput.text, out float parsedLength))
                {
                    Debug.LogWarning("Invalid length input. Using default value of 1.");
                    parsedLength = 1f;
                    lengthInput.text = parsedLength.ToString();
                }

                // Set the parsed values to the config
                config.DefaultIterations = Mathf.RoundToInt(iterationSlider.value);
                config.Angle = parsedAngle;
                config.Length = parsedLength;

                // Set values for the custom parameters
                if (!float.TryParse(lengthVariationInput.text, out float lengthVariation))
                {
                    lengthVariation = 0.2f; // Default value
                }
                config.LengthVariationFactor = lengthVariation;

                if (!float.TryParse(thicknessVariationInput.text, out float thicknessVariation))
                {
                    thicknessVariation = 0.3f; // Default value
                }
                config.ThicknessVariationFactor = thicknessVariation;

                if (!float.TryParse(curvatureAngleInput.text, out float curvatureAngle))
                {
                    curvatureAngle = 10f; // Default value
                }
                config.CurvatureAngle = curvatureAngle;

                if (!float.TryParse(leafPlacementProbabilityInput.text, out float leafProbability))
                {
                    leafProbability = 1f; // Default value
                }
                config.LeafPlacementProbability = Mathf.Clamp01(leafProbability);

                rotationSlider.value = 0;

                OnGenerateRequested?.Invoke(config);

                // Find the renderer and subscribe to OnRenderComplete to re-enable UI
                var renderer = FindObjectOfType<LSystemRenderer>();
                if (renderer != null)
                {
                    renderer.OnRenderComplete += EnableUIAfterGeneration;
                    renderer.Render(config);
                }
            }
            else
            {
                Debug.LogError("LSystemUIController: Selected config index is out of range.");
            }
        }

        // Method to re-enable both the generate button and rotation slider after rendering completes
        private void EnableUIAfterGeneration()
        {
            generateButton.interactable = true;
            rotationSlider.interactable = true;
            Debug.Log("UI elements re-enabled after L-System generation.");

            // Unsubscribe from the OnRenderComplete event to prevent multiple calls
            var renderer = FindObjectOfType<LSystemRenderer>();
            if (renderer != null)
            {
                renderer.OnRenderComplete -= EnableUIAfterGeneration;
            }
        }


        // Method to re-enable the generate button when rendering is complete
        private void EnableGenerateButton()
        {
            generateButton.interactable = true; // Re-enable the button after rendering completes
            var renderer = FindObjectOfType<LSystemRenderer>();
            if (renderer != null)
            {
                renderer.OnRenderComplete -= EnableGenerateButton; // Unsubscribe to avoid multiple triggers
            }
        }


        private void EnableRotationSlider()
        {
            rotationSlider.interactable = true; // Enable the slider after rendering completes
            var renderer = FindObjectOfType<LSystemRenderer>();
            if (renderer != null)
            {
                renderer.OnRenderComplete -= EnableRotationSlider; // Unsubscribe to avoid multiple triggers
            }
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
            leafVariantDropdown.onValueChanged.RemoveListener(OnLeafVariantDropdownChanged);

            rotationSlider.onValueChanged.RemoveListener(OnRotationSliderChanged);

            lengthVariationInput.onEndEdit.RemoveListener(OnLengthVariationChanged);
            thicknessVariationInput.onEndEdit.RemoveListener(OnThicknessVariationChanged);
            curvatureAngleInput.onEndEdit.RemoveListener(OnCurvatureAngleChanged);
            leafPlacementProbabilityInput.onEndEdit.RemoveListener(OnLeafPlacementProbabilityChanged);
        }
    }
}
