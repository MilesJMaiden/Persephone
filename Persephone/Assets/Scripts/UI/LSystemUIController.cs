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
        [Header("Rules Display")]
        [SerializeField]
        private TMP_Text rulesDisplayText;

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

        [SerializeField]
        private TMP_Dropdown flowerVariantDropdown;

        [SerializeField]
        private Toggle windToggle;

        [Header("Wind Settings UI")]
        [SerializeField]
        private Slider windStrengthSlider;

        [SerializeField]
        private Slider windFrequencySlider;

        [SerializeField]
        private Slider gustinessSlider;

        [SerializeField]
        private TMP_InputField windDirectionX;

        [SerializeField]
        private TMP_InputField windDirectionY;

        [SerializeField]
        private TMP_InputField windDirectionZ;

        [SerializeField]
        private TMP_InputField lengthVariationInput;

        [SerializeField]
        private TMP_InputField thicknessVariationInput;

        [SerializeField]
        private TMP_InputField curvatureAngleInput;

        [SerializeField]
        private TMP_InputField leafPlacementProbabilityInput;

        [Header("Leaf Settings UI")]
        [SerializeField]
        private Toggle leavesToggle;

        [SerializeField] private TMP_InputField leafScaleMinInput;
        [SerializeField] private TMP_InputField leafScaleMaxInput;
        //[SerializeField] private TMP_InputField leafPlacementProbabilityInput;
        [SerializeField] private TMP_InputField leafDensityInput;

        [Header("Leaf Color Settings")]
        [SerializeField] private TMP_InputField leafColorInput;

        [Header("Flower Settings UI")]
        [SerializeField]
        private Toggle flowersToggle;
        [SerializeField] private TMP_InputField flowerScaleMinInput;
        [SerializeField] private TMP_InputField flowerScaleMaxInput;
        [SerializeField] private TMP_InputField flowerPlacementProbabilityInput;
        [SerializeField] private TMP_InputField flowerOffsetInput;

        [Header("Flower Color Settings")]
        [SerializeField] private TMP_InputField flowerColorInput;

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
            InitializeFlowerVariantDropdown();
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

            windToggle.onValueChanged.AddListener(OnWindToggleChanged);

            windStrengthSlider.onValueChanged.AddListener(OnWindStrengthChanged);
            windFrequencySlider.onValueChanged.AddListener(OnWindFrequencyChanged);
            gustinessSlider.onValueChanged.AddListener(OnGustinessChanged);
            windDirectionX.onEndEdit.AddListener(OnWindDirectionChanged);
            windDirectionY.onEndEdit.AddListener(OnWindDirectionChanged);
            windDirectionZ.onEndEdit.AddListener(OnWindDirectionChanged);

            leafScaleMinInput.onEndEdit.AddListener(OnLeafScaleMinChanged);
            leafScaleMaxInput.onEndEdit.AddListener(OnLeafScaleMaxChanged);
            leafPlacementProbabilityInput.onEndEdit.AddListener(OnLeafPlacementProbabilityChanged);
            leafDensityInput.onEndEdit.AddListener(OnLeafDensityChanged);
            leafColorInput.onEndEdit.AddListener(OnLeafColorCodeChanged);

            flowerScaleMinInput.onEndEdit.AddListener(OnFlowerScaleMinChanged);
            flowerScaleMaxInput.onEndEdit.AddListener(OnFlowerScaleMaxChanged);
            flowerPlacementProbabilityInput.onEndEdit.AddListener(OnFlowerPlacementProbabilityChanged);
            flowerOffsetInput.onEndEdit.AddListener(OnFlowerOffsetChanged);
            flowerColorInput.onEndEdit.AddListener(OnFlowerColorCodeChanged);

            leavesToggle.onValueChanged.AddListener(OnLeavesToggleChanged);
            flowersToggle.onValueChanged.AddListener(OnFlowersToggleChanged);

            rotationSlider.interactable = false;

            // Ensure the initial UI matches the first selected configuration
            if (variantDropdown.options.Count > 0 && variantDropdown.value >= 0)
            {
                UpdateUIWithConfig(lSystemConfigs[variantDropdown.value]);
                DisplayRules(lSystemConfigs[variantDropdown.value]);
            }
        }

        private void DisplayRules(LSystemConfig config)
        {
            if (config == null || rulesDisplayText == null) return;

            string rulesText = $"Axiom: {config.Axiom}\n\nRules:\n";

            foreach (var rule in config.Rules)
            {
                rulesText += $"Predecessor: {rule.Predecessor}\nSuccessor: {rule.Successor}\n\n";
            }

            rulesDisplayText.text = rulesText;
        }

        private void InitializeFlowerVariantDropdown()
        {
            flowerVariantDropdown.ClearOptions();

            var lSystemRenderer = FindObjectOfType<LSystemRenderer>();
            if (lSystemRenderer != null && lSystemRenderer.flowerVariants.Length > 0)
            {
                var flowerOptions = new List<string>();
                foreach (var flowerVariant in lSystemRenderer.flowerVariants)
                {
                    if (flowerVariant != null)
                    {
                        flowerOptions.Add(flowerVariant.name);
                    }
                }
                flowerVariantDropdown.AddOptions(flowerOptions);
                flowerVariantDropdown.value = lSystemConfigs[variantDropdown.value].SelectedFlowerVariantIndex; // Set initial value
                flowerVariantDropdown.onValueChanged.AddListener(OnFlowerVariantDropdownChanged);
            }
            else
            {
                Debug.LogWarning("LSystemRenderer or flowerVariants not set correctly.");
            }
        }

        private void OnFlowerVariantDropdownChanged(int index)
        {
            var config = GetSelectedConfig();
            if (config != null)
            {
                config.SelectedFlowerVariantIndex = index; // Update the selected index in the config
            }

            Debug.Log($"Flower Variant Selected: {index}");
        }

        public void RegeneratePlant()
        {
            Debug.Log("Regenerating plant.");
            OnGenerateButtonClicked();
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

        private void OnLeavesToggleChanged(bool isOn)
        {
            var lSystemRenderer = FindObjectOfType<LSystemRenderer>();
            if (lSystemRenderer != null)
            {
                lSystemRenderer.SetLeavesActive(isOn);
            }
            Debug.Log($"Leaves visibility toggled: {(isOn ? "On" : "Off")}");
        }

        private void OnFlowersToggleChanged(bool isOn)
        {
            var lSystemRenderer = FindObjectOfType<LSystemRenderer>();
            if (lSystemRenderer != null)
            {
                lSystemRenderer.SetFlowersActive(isOn);
            }
            Debug.Log($"Flowers visibility toggled: {(isOn ? "On" : "Off")}");
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
            yield return new WaitUntil(() => renderer.IsRenderingComplete);
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
            if (lSystemRenderer != null && lSystemRenderer.IsRenderingComplete)
            {
                lSystemRenderer.RotateRenderer(value);
            }
            else
            {
                rotationSlider.value = 0;
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
                DisplayRules(lSystemConfigs[index]);
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


            // Update UI elements with values from the config
            leafScaleMinInput.text = config.LeafScaleMin.ToString();
            leafScaleMaxInput.text = config.LeafScaleMax.ToString();
            leafPlacementProbabilityInput.text = config.LeafPlacementProbability.ToString();
            leafDensityInput.text = config.LeafDensity.ToString();

            flowerScaleMinInput.text = config.FlowerScaleMin.ToString();
            flowerScaleMaxInput.text = config.FlowerScaleMax.ToString();
            flowerPlacementProbabilityInput.text = config.FlowerPlacementProbability.ToString();
            flowerOffsetInput.text = config.FlowerOffset.ToString();

            if (flowerVariantDropdown != null)
            {
                flowerVariantDropdown.value = config.SelectedFlowerVariantIndex;
            }

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

        private void OnLeafScaleMinChanged(string value)
        {
            if (float.TryParse(value, out float parsedValue))
            {
                lSystemConfigs[variantDropdown.value].LeafScaleMin = parsedValue;
            }
        }
        private void OnLeafScaleMaxChanged(string value)
        {
            UpdateSelectedConfig(value, (config, v) => config.LeafScaleMax = Mathf.Max(v, config.LeafScaleMin));
        }
        private void OnLeafPlacementProbabilityChanged(string value)
        {
            UpdateSelectedConfig(value, (config, v) => config.LeafPlacementProbability = Mathf.Clamp01(v));
        }
        private void OnLeafDensityChanged(string value)
        {
            UpdateSelectedConfig(value, (config, v) => config.LeafDensity = Mathf.Clamp(v, 0.1f, 2.0f));
        }

        private void OnLeafColorChanged(Color color)
        {
            lSystemConfigs[variantDropdown.value].LeafColor = color;
        }

        private void OnFlowerScaleMinChanged(string value)
        {
            UpdateSelectedConfig(value, (config, v) => config.FlowerScaleMin = Mathf.Min(v, config.FlowerScaleMax));
        }

        private void OnFlowerScaleMaxChanged(string value)
        {
            UpdateSelectedConfig(value, (config, v) => config.FlowerScaleMax = Mathf.Max(v, config.FlowerScaleMin));
        }
        private void OnFlowerPlacementProbabilityChanged(string value)
        {
            if (float.TryParse(value, out float probability))
            {
                lSystemConfigs[variantDropdown.value].FlowerPlacementProbability = Mathf.Clamp01(probability);
            }
        }
        private void OnFlowerOffsetChanged(string value)
        {
            UpdateSelectedConfig(value, (config, v) => config.FlowerOffset = Mathf.Clamp(v, 0f, 0.1f));
        }
        private void OnFlowerColorChanged(Color color)
        {
            var config = GetSelectedConfig();
            if (config != null)
            {
                config.FlowerColor = color;
            }
        }

        private void OnLeafColorCodeChanged(string colorCode)
        {
            if (ColorUtility.TryParseHtmlString(colorCode, out Color parsedColor))
            {
                lSystemConfigs[variantDropdown.value].LeafColor = parsedColor;
            }
            else
            {
                Debug.LogWarning($"Invalid leaf color code: {colorCode}");
            }
        }

        private void OnFlowerColorCodeChanged(string colorCode)
        {
            if (ColorUtility.TryParseHtmlString(colorCode, out Color parsedColor))
            {
                lSystemConfigs[variantDropdown.value].FlowerColor = parsedColor;
            }
            else
            {
                Debug.LogWarning($"Invalid flower color code: {colorCode}");
            }
        }

        private void UpdateSelectedConfig(string value, Action<LSystemConfig, float> updateAction)
        {
            var config = GetSelectedConfig();
            if (config == null)
            {
                Debug.LogWarning("No L-System configuration is selected.");
                return;
            }

            if (!float.TryParse(value, out float parsedValue))
            {
                Debug.LogWarning($"Invalid input: {value}. Skipping update.");
                return;
            }

            updateAction.Invoke(config, parsedValue);
        }

        private LSystemConfig GetSelectedConfig()
        {
            if (lSystemConfigs != null && variantDropdown.value >= 0 && variantDropdown.value < lSystemConfigs.Length)
            {
                return lSystemConfigs[variantDropdown.value];
            }

            Debug.LogWarning("Invalid or no L-System configuration selected.");
            return null;
        }

        private void OnGenerateButtonClicked()
        {
            generateButton.interactable = false;
            rotationSlider.interactable = false;
            windToggle.isOn = false;

            int selectedIndex = variantDropdown.value;

            if (selectedIndex >= 0 && selectedIndex < lSystemConfigs.Length)
            {
                LSystemConfig config = lSystemConfigs[selectedIndex];

                if (!float.TryParse(angleInput.text, out float parsedAngle))
                {
                    parsedAngle = 25f;
                    angleInput.text = parsedAngle.ToString();
                }

                if (!float.TryParse(lengthInput.text, out float parsedLength))
                {
                    parsedLength = 1f;
                    lengthInput.text = parsedLength.ToString();
                }

                config.DefaultIterations = Mathf.RoundToInt(iterationSlider.value);
                config.Angle = parsedAngle;
                config.Length = parsedLength;

                OnGenerateRequested?.Invoke(config);

                var renderer = FindObjectOfType<LSystemRenderer>();
                if (renderer != null)
                {
                    renderer.OnRenderComplete += EnableUIAfterGeneration;
                    renderer.Render(config);
                }
            }
        }

        private void EnableUIAfterGeneration()
        {
            generateButton.interactable = true;
            rotationSlider.interactable = true;

            var renderer = FindObjectOfType<LSystemRenderer>();
            if (renderer != null)
            {
                renderer.OnRenderComplete -= EnableUIAfterGeneration;
            }
        }

        private void OnWindToggleChanged(bool isOn)
        {
            var windManager = FindObjectOfType<WindManager>();
            if (windManager != null)
            {
                windManager.ToggleWind(isOn);
            }
        }

        private void OnWindStrengthChanged(float value)
        {
            var windManager = FindObjectOfType<WindManager>();
            if (windManager != null)
            {
                windManager.WindStrength = value;
            }
        }

        private void OnWindFrequencyChanged(float value)
        {
            var windManager = FindObjectOfType<WindManager>();
            if (windManager != null)
            {
                windManager.WindFrequency = value;
            }
        }

        private void OnGustinessChanged(float value)
        {
            var windManager = FindObjectOfType<WindManager>();
            if (windManager != null)
            {
                windManager.Gustiness = value;
            }
        }

        private void OnWindDirectionChanged(string value)
        {
            if (float.TryParse(windDirectionX.text, out float x) &&
                float.TryParse(windDirectionY.text, out float y) &&
                float.TryParse(windDirectionZ.text, out float z))
            {
                var windManager = FindObjectOfType<WindManager>();
                if (windManager != null)
                {
                    windManager.WindDirection = new Vector3(x, y, z).normalized;
                }
            }
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

            windToggle.onValueChanged.RemoveListener(OnWindToggleChanged);
            windStrengthSlider.onValueChanged.RemoveListener(OnWindStrengthChanged);
            windFrequencySlider.onValueChanged.RemoveListener(OnWindFrequencyChanged);
            gustinessSlider.onValueChanged.RemoveListener(OnGustinessChanged);
            windDirectionX.onEndEdit.RemoveListener(OnWindDirectionChanged);
            windDirectionY.onEndEdit.RemoveListener(OnWindDirectionChanged);
            windDirectionZ.onEndEdit.RemoveListener(OnWindDirectionChanged);
        }
    }
}
