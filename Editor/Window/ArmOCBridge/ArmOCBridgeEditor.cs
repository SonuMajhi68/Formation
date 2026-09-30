using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;

using Debug = UnityEngine.Debug;
using Object = UnityEngine.Object;
using ObjectField = UnityEditor.UIElements.ObjectField;

namespace Formation.ArmOCBridge
{
    public class ArmOCBridgeEditor : EditorWindow
    {
        private int _index = 0;
        private string _passVal;
        private string _keywordVal;
        private Shader _choosenShader;
        private MaliOCConfig config;
        private List<CompiledShaderResult> _compiledResults;
        private List<CompiledShaderResult> _filterdResults;
        private readonly List<string> gpuModels = new List<string> {
        "Mali-G76", "Mali-G72", "Mali-G71", "Mali-G52", "Mali-G51", "Mali-G31",                                                                                             // Bifrost Architecture
        "Immortalis-G715", "Mali-G715", "Mali-G710", "Mali-G615", "Mali-G610", "Mali-G510", "Mali-G310", "Mali-G78AE", "Mali-G78", "Mali-G77", "Mali-G68", "Mali-G57",      // Valhall Architecture
        "Mali-G2", "Mali-G1", "Immortalis-G925", "Immortalis-G720", "Mali-G725", "Mali-G720", "Mali-G625", "Mali-G620"                                                      // Arm 5th Generation Architecture
    };


        // Configuration Fold-Out
        private Toggle _detailViw;
        private Toggle _autoSave;
        private Toggle _suggestion;
        private Button _browsePath;
        private Button _tempPathBtn;
        private TextField _compilerPath;
        private TextField _tempPath;
        private DropdownField _gpuModel;


        // Shader Selection Fold-Out
        private Button _compileBtn;
        private ObjectField _shader;


        // Varient Selection Fold-Out
        private Label _compiledVertex;
        private Label _compiledFragment;
        private Label _currVarientCount;
        private Label _totalVarientCount;
        private Label _currentPass;
        private Label _currentKeywords;
        private Button _searchFilterBtn;
        private Button _clearFilterBtn;
        private Button _prevVarientBtn;
        private Button _nextVarientBtn;
        private Button _copyVertBtn;
        private Button _copyFragBtn;
        private TextField _passName;
        private TextField _keyword;


        // Analysis Report Fold-Out
        private Button _analyzeBtn;
        private Button _saveReportBtn;
        private Button _clearReportBtn;
        private Label _analysedVertex;
        private Label _analysedFragment;
        private VisualElement _suggestionBlock;



        [MenuItem("Tools/Arm Offline Compiler Bridge")]
        public static void OpenEditorWindow()
        {
            ArmOCBridgeEditor window = GetWindow<ArmOCBridgeEditor>();
            window.titleContent = new GUIContent(text: "Arm Offline Compiler Bridge");
        }

        private void OnEnable()
        {
            config = MaliOCConfig.Load();
        }

        private void CreateGUI()
        {
            VisualElement root = rootVisualElement;

            string path = "Assets/Extension/Editor/Window/ArmOCBridge/ArmOCBridgeEditorWindow.uxml";
            VisualTreeAsset visualTree = AssetDatabase.LoadAssetAtPath<VisualTreeAsset>(path);

            if (visualTree == null)
            {
                //Debug.Log("1");
                path = "Packages/com.sonumajhi68.formation/Editor/Window/ArmOCBridge/ArmOCBridgeEditorWindow.uxml";
                visualTree = AssetDatabase.LoadAssetAtPath<VisualTreeAsset>(path);
            }

            VisualElement tree = visualTree.Instantiate();
            root.Add(tree);


            // Assign Elements

            _compilerPath = root.Q<TextField>("browsePath");
            _browsePath = root.Q<Button>("browseBtn");
            _tempPath = root.Q<TextField>("folderPath");
            _tempPathBtn = root.Q<Button>("folderPathBtn");
            _gpuModel = root.Q<DropdownField>("gpuModel");
            _detailViw = root.Q<Toggle>("detailToggle");
            _autoSave = root.Q<Toggle>("saveToggle");
            _autoSave = root.Q<Toggle>("saveToggle");
            _suggestion = root.Q<Toggle>("suggestionToggle");

            _shader = root.Q<ObjectField>("selectedShader");
            _compileBtn = root.Q<Button>("compileBtn");

            _passName = root.Q<TextField>("pass");
            _keyword = root.Q<TextField>("keyword");
            _searchFilterBtn = root.Q<Button>("searchBtn");
            _clearFilterBtn = root.Q<Button>("clearBtn");
            _compiledVertex = root.Q<Label>("vertexComp");
            _compiledFragment = root.Q<Label>("fragmentComp");
            _currVarientCount = root.Q<Label>("currVar");
            _totalVarientCount = root.Q<Label>("totalVar");
            _currentPass = root.Q<Label>("currPass");
            _currentKeywords = root.Q<Label>("currKeyword");
            _prevVarientBtn = root.Q<Button>("prevBtn");
            _nextVarientBtn = root.Q<Button>("nextBtn");
            _copyVertBtn = root.Q<Button>("copyVert");
            _copyFragBtn = root.Q<Button>("copyFrag");

            _analyzeBtn = root.Q<Button>("analyzeBtn");
            _saveReportBtn = root.Q<Button>("saveBtn");
            _clearReportBtn = root.Q<Button>("clearReportBtn");

            _analysedVertex = root.Q<Label>("vertexResult");
            _analysedFragment = root.Q<Label>("fragmentResult");
            _suggestionBlock = root.Q<VisualElement>("Suggestion-block");


            // Initiate Value

            _gpuModel.choices = gpuModels;

            _searchFilterBtn.SetEnabled(false);
            _clearFilterBtn.SetEnabled(false);
            _prevVarientBtn.SetEnabled(false);
            _nextVarientBtn.SetEnabled(false);
            _copyVertBtn.SetEnabled(false);
            _copyFragBtn.SetEnabled(false);

            _analyzeBtn.SetEnabled(false);
            _saveReportBtn.SetEnabled(false);
            _clearReportBtn.SetEnabled(false);


            // Assign Values

            _compilerPath.value = config.compilerPath;
            _tempPath.value = config.tempFilePath;
            _gpuModel.value = config.selectedGPUModel;
            _detailViw.value = config.detailedOutput;
            _autoSave.value = config.autoSaveResults;
            _suggestion.value = config.showSuggestion;
            _suggestionBlock.style.display = (config.showSuggestion) ? DisplayStyle.Flex : DisplayStyle.None;


            // Assign Callback

            _browsePath.clicked += SelectPath;
            _tempPathBtn.clicked += SelectTempPath;
            _gpuModel.RegisterValueChangedCallback<string>(SelectGPU);
            _detailViw.RegisterValueChangedCallback<bool>(ToggleDetailView);
            _autoSave.RegisterValueChangedCallback<bool>(ToggleAutoSave);
            _suggestion.RegisterValueChangedCallback<bool>(ToggleShowSuggestion);

            _shader.RegisterValueChangedCallback<Object>(SelectShader);
            _compileBtn.clicked += () => CompileShader(_choosenShader);

            _passName.RegisterValueChangedCallback<String>(FilterPass);
            _keyword.RegisterValueChangedCallback<String>(FilterKeyword);

            _searchFilterBtn.clicked += SearchFilter;
            _clearFilterBtn.clicked += ClearFilter;
            _prevVarientBtn.clicked += prevVarient;
            _nextVarientBtn.clicked += nextVarient;
            _copyVertBtn.clicked += CopyVertex;
            _copyFragBtn.clicked += CopyFragment;

            _analyzeBtn.clicked += AnalyzeCode;
            _saveReportBtn.clicked += SaveReport;
            _clearReportBtn.clicked += ClearReport;

        }


        #region Analysis Block

        private void AnalyzeCode()
        {
            string tempDir;
            tempDir = (config.tempFilePath != "") ? config.tempFilePath : Path.Combine(Application.temporaryCachePath, "ArmOCBridge");


            if (!Directory.Exists(tempDir)) Directory.CreateDirectory(tempDir);

            string vertexFile = Path.Combine(tempDir, "vertex.vert");
            string fragmentFile = Path.Combine(tempDir, "fragment.frag");

            File.WriteAllText(vertexFile, _compiledResults[_index].vertexShader);
            File.WriteAllText(fragmentFile, _compiledResults[_index].fragmentShader);

            string vertexResult = CompileShaderFile(vertexFile, "Vertex");
            string fragmentResult = CompileShaderFile(fragmentFile, "Fragment");

            _analysedVertex.text = vertexResult;
            _analysedFragment.text = fragmentResult;

            if (config.autoSaveResults)
            {
                SaveReport();
            }

            try
            {
                File.Delete(vertexFile);
                File.Delete(fragmentFile);
            }
            catch { }

            _saveReportBtn.SetEnabled(true);
            _clearReportBtn.SetEnabled(true);
        }


        private void SaveReport()
        {
            if (string.IsNullOrEmpty(_analysedVertex.text) && string.IsNullOrEmpty(_analysedVertex.text)) return;


            string saveDir = (config.tempFilePath != "") ? config.tempFilePath : Path.Combine(Application.temporaryCachePath, "ArmOCBridge");

            if (!Directory.Exists(saveDir))
                Directory.CreateDirectory(saveDir);

            string fileName = $"ArmOCBridge_{_choosenShader.name.Replace("/", "_")}_{DateTime.Now:yyyyMMdd_HHmmss}.txt";
            string path = Path.Combine(saveDir, fileName);

            StringBuilder keywords = new StringBuilder();
            foreach (var keyword in _filterdResults[_index].keywords) keywords.Append($"{keyword.ToString()} ");

            StringBuilder fullReport = new StringBuilder();
            fullReport.AppendLine($"Mali Compiler save report");
            fullReport.AppendLine($"Shader: {_choosenShader.name}");
            fullReport.AppendLine($"Generation Time: {DateTime.Now}");
            fullReport.AppendLine($"Pass: {_filterdResults[_index].pass}");
            fullReport.AppendLine($"Keyword: {keywords.ToString()}");
            fullReport.AppendLine("\n\n");
            fullReport.AppendLine(_analysedVertex.text);
            fullReport.AppendLine("\n\n");
            fullReport.AppendLine(_analysedFragment.text);

            File.WriteAllText(path, fullReport.ToString());

            Debug.Log("Report Saved!");
        }


        private void ClearReport()
        {
            _analysedVertex.text = "";
            _analysedFragment.text = "";

            _clearReportBtn.SetEnabled(false);
        }


        private string CompileShaderFile(string filePath, string shaderType)
        {
            try
            {
                var startInfo = new ProcessStartInfo
                {
                    FileName = config.compilerPath,
                    UseShellExecute = false,
                    RedirectStandardOutput = true,
                    RedirectStandardError = true,
                    CreateNoWindow = true
                };

                startInfo.Arguments = $"\"{filePath}\" -c {config.selectedGPUModel}";

                if (config.detailedOutput)
                {
                    startInfo.Arguments += " -d";
                }

                using (var process = Process.Start(startInfo))
                {
                    string output = process.StandardOutput.ReadToEnd();
                    string error = process.StandardError.ReadToEnd();

                    process.WaitForExit(30000);

                    string uiFriendlyOutput = output.Replace(' ', '\u00A0');

                    if (process.ExitCode == 0)
                    {
                        //return $"=== {shaderType} Shader Compilation successful ===\n\n{output}";
                        return $"=== {shaderType} Shader Compilation successful ===\n\n{uiFriendlyOutput}";
                    }
                    else
                    {
                        //return $"=== {shaderType} Shader Compilation failed ===\n\nFailed: {error}\n\n output: {output}";
                        return $"=== {shaderType} Shader Compilation failed ===\n\nFailed: {error}\n\n output: {uiFriendlyOutput}";
                    }
                }
            }
            catch (Exception e)
            {
                return $"=== {shaderType} Shader Compilation exception ===\n\n{e.Message}";
            }
        }

        #endregion

        #region Varient Selection Block

        // Copy Shader disassembly
        private void CopyFragment()
        {
            GUIUtility.systemCopyBuffer = _compiledResults[_index].fragmentShader;
        }


        private void CopyVertex()
        {
            GUIUtility.systemCopyBuffer = _compiledResults[_index].vertexShader;
        }


        // Swtich between varient
        private void prevVarient()
        {
            _index = _index - 1;

            PopulateVarient();
        }


        private void nextVarient()
        {
            _index = _index + 1;

            PopulateVarient();
        }


        // Filter out the Query
        private void SearchFilter()
        {
            _filterdResults = new List<CompiledShaderResult>();


            if (_passVal == "" && _keywordVal == "")
            {
                _filterdResults = _compiledResults;

                PopulateVarient();

                return;
            }

            foreach (var item in _compiledResults)
            {
                bool passMatches = false;
                bool keywordMatches = false;

                if (_passVal == "") passMatches = true;
                if (_keywordVal == "") keywordMatches = true;
                if (!string.IsNullOrEmpty(item.pass) && item.pass.Contains(_passVal)) passMatches = true;

                if (item.keywords != null)
                {
                    foreach (var keyword in item.keywords)
                    {
                        //Debug.Log(keyword);
                        if (!string.IsNullOrEmpty(keyword) && keyword.Contains(_keywordVal))
                        {
                            keywordMatches = true;
                            break;                      // Found a match, no need to keep checking remaining keywords
                        }
                    }
                }

                if (passMatches && keywordMatches) _filterdResults.Add(item);
            }

            _index = 0;

            PopulateVarient();
        }


        private void ClearFilter()
        {
            _passName.value = "";
            _keyword.value = "";
        }


        // Filter Field handler
        private void FilterKeyword(ChangeEvent<string> evt)
        {
            _keywordVal = evt.newValue;

            bool isFilterEmpty = string.IsNullOrWhiteSpace(_passVal) && string.IsNullOrWhiteSpace(_keywordVal);
            _clearFilterBtn.SetEnabled(!isFilterEmpty);
        }


        private void FilterPass(ChangeEvent<string> evt)
        {
            _passVal = evt.newValue;

            bool isFilterEmpty = string.IsNullOrWhiteSpace(_passVal) && string.IsNullOrWhiteSpace(_keywordVal);
            _clearFilterBtn.SetEnabled(!isFilterEmpty);
        }

        #endregion

        #region Shader Selection Block

        private void SelectShader(ChangeEvent<Object> evt)
        {
            _choosenShader = evt.newValue as Shader;
        }


        private void CompileShader(Shader choosenShader)
        {
            if (choosenShader == null) return;

            _compiledResults = ArmOCBridgeShaderCompiler.ExtractAllVariants(choosenShader);
            _filterdResults = _compiledResults; // test
            _index = 0;

            PopulateVarient();

            _searchFilterBtn.SetEnabled(true);
            _copyVertBtn.SetEnabled(true);
            _copyFragBtn.SetEnabled(true);
            _analyzeBtn.SetEnabled(true);
        }

        #endregion

        #region Configuration Block

        private void SelectPath()
        {
            string path = EditorUtility.OpenFilePanel("Select Mali Compiler", "", "exe");
            if (!string.IsNullOrEmpty(path))
            {
                _compilerPath.value = path;

                config.compilerPath = path;
                config.Save();
            }
        }


        private void SelectTempPath()
        {
            string path = EditorUtility.OpenFolderPanel("Select Foder", "", "");
            if (!string.IsNullOrEmpty(path))
            {
                _tempPath.value = path;

                config.tempFilePath = path;
                config.Save();
            }
        }


        private void SelectGPU(ChangeEvent<string> evt)
        {
            string gpuSelected = evt.newValue;

            if (gpuSelected != null)
            {
                config.selectedGPUModel = gpuSelected;
                config.Save();
            }
        }


        private void ToggleDetailView(ChangeEvent<bool> evt)
        {
            config.detailedOutput = evt.newValue;
            config.Save();
        }


        private void ToggleAutoSave(ChangeEvent<bool> evt)
        {
            config.autoSaveResults = evt.newValue;
            config.Save();
        }


        private void ToggleShowSuggestion(ChangeEvent<bool> evt)
        {
            config.showSuggestion = evt.newValue;
            _suggestionBlock.style.display = (evt.newValue) ? DisplayStyle.Flex : DisplayStyle.None;

            config.Save();
        }

        #endregion

        #region Helper

        private void PopulateVarient()
        {
            var val = _filterdResults[_index];

            string cVert = val.vertexShader.Replace(' ', '\u00A0');
            string cFrag = val.fragmentShader.Replace(' ', '\u00A0');

            _compiledVertex.text = cVert;
            _compiledFragment.text = cFrag;
            _currentPass.text = val.pass;

            StringBuilder txt = new StringBuilder();
            foreach (var c in val.keywords)
            {
                txt.Append($"{c}\n");
            }

            _currentKeywords.text = txt.ToString().Trim();
            _currVarientCount.text = (_index + 1).ToString();
            _totalVarientCount.text = _filterdResults.Count.ToString();

            if (_filterdResults.Count - 1 == _index)
                _nextVarientBtn.SetEnabled(false);
            else
                _nextVarientBtn.SetEnabled(true);


            if (_index > 0)
                _prevVarientBtn.SetEnabled(true);
            else
                _prevVarientBtn.SetEnabled(false);
        }

        #endregion
    }


    public class MaliOCConfig
    {
        public string compilerPath = "";
        public string selectedGPUModel = "Mali-G78";
        public bool autoSaveResults = true;
        public bool showSuggestion = true;
        public bool detailedOutput = true;
        public string tempFilePath = "";


        public static MaliOCConfig Load()
        {
            MaliOCConfig config = new MaliOCConfig();

            config.compilerPath = UnityEditor.EditorPrefs.GetString("MaliOC_compPath", "");
            config.selectedGPUModel = UnityEditor.EditorPrefs.GetString("MaliOC_gpuModel", "Mali-G78");
            config.autoSaveResults = UnityEditor.EditorPrefs.GetBool("MaliOC_autoSave", false);
            config.showSuggestion = UnityEditor.EditorPrefs.GetBool("MaliOC_showSuggestion", false);
            config.detailedOutput = UnityEditor.EditorPrefs.GetBool("MaliOC_detailedOutput", false);
            config.tempFilePath = UnityEditor.EditorPrefs.GetString("MaliOC_tempPath", "");

            return config;
        }


        public void Save()
        {
            UnityEditor.EditorPrefs.SetString("MaliOC_compPath", compilerPath);
            UnityEditor.EditorPrefs.SetString("MaliOC_gpuModel", selectedGPUModel);
            UnityEditor.EditorPrefs.SetBool("MaliOC_autoSave", autoSaveResults);
            UnityEditor.EditorPrefs.SetBool("MaliOC_showSuggestion", showSuggestion);
            UnityEditor.EditorPrefs.SetBool("MaliOC_detailedOutput", detailedOutput);
            UnityEditor.EditorPrefs.SetString("MaliOC_tempPath", tempFilePath);
        }


        public void ResetToDefault()
        {
            compilerPath = "";
            selectedGPUModel = "Mali-G78";
            autoSaveResults = false;
            showSuggestion = true;
            detailedOutput = true;
            tempFilePath = "";
        }
    }
}
