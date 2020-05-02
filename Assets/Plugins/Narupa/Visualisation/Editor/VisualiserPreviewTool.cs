using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using Narupa.Frame;
using Narupa.Frame.Import.CIF;
using Narupa.Frame.Import.CIF.Components;
using Narupa.Visualisation.Components;
using Narupa.Visualisation.Node.Adaptor;
using Narupa.Visualisation.Properties;
using NarupaIMD.Selection;
using UnityEditor;
using UnityEditor.EditorTools;
using UnityEditorInternal;
using UnityEngine;
using Valve.Newtonsoft.Json.Linq;

namespace Narupa.Visualisation.Editor
{
    // Tagging a class with the EditorTool attribute and no target type registers a global tool. Global tools are valid for any selection, and are accessible through the top left toolbar in the editor.
    [EditorTool("Visualiser Preview")]
    class VisualiserPreviewTool : EditorTool
    {
        // Serialize this value to set a default value in the Inspector.
        [SerializeField]
        Texture2D m_ToolIcon;

        GUIContent m_IconContent;

        public override GUIContent toolbarIcon => m_IconContent;

        public static VisualiserPreviewTool Instance { get; private set; }

        public GameObject CurrentVisualiser => currentVisualiser;

        public void OnEnable()
        {
            Instance = this;

            m_IconContent = new GUIContent()
            {
                image = m_ToolIcon,
                text = "Visualiser Preview",
                tooltip = "Visualiser Preview"
            };

            Selection.selectionChanged += RefreshFromSelection;
            EditorTools.activeToolChanged += EditorToolsOnactiveToolChanged;
        }

        private GUIStyle frameBoxStyle;

        private void EditorToolsOnactiveToolChanged()
        {
            if (EditorTools.IsActiveTool(this))
                Activate();
            else
                Deactivate();
        }

        private FileSystemWatcher watcher;
        private string customVisualiserJson;

        private void SetupFileWatcher()
        {
            jsonTempFileName = Path.Combine(Directory.GetCurrentDirectory(),
                                            "Temp/visualiser.json");
            if (!File.Exists(jsonTempFileName))
            {
                using (var file = File.Create(jsonTempFileName))
                using (var writer = new StreamWriter(file))
                {
                    writer.Write("{\n}");
                }

                customVisualiserJson = "{}";
            }
            else
            {
                using (var stream = File.OpenRead(jsonTempFileName))
                using (var reader = new StreamReader(stream))
                {
                    customVisualiserJson = reader.ReadToEnd();
                }
            }

            var path = Path.Combine(Directory.GetCurrentDirectory(),
                                    "Temp");
            var filename = Path.GetFileName(jsonTempFileName);
            watcher = new FileSystemWatcher(path, filename);
            watcher.Changed += WatcherOnChanged;
            watcher.EnableRaisingEvents = true;
        }

        private void WatcherOnChanged(object sender, FileSystemEventArgs e)
        {
            using (var stream = File.OpenRead(e.FullPath))
            using (var reader = new StreamReader(stream))
            {
                customVisualiserJson = reader.ReadToEnd();
            }
        }

        private void RemoveFileWatcher()
        {
            watcher.Dispose();
            watcher = null;
        }

        public void OnDisable()
        {
            Selection.selectionChanged -= RefreshFromSelection;
            EditorTools.activeToolChanged -= EditorToolsOnactiveToolChanged;
            Instance = null;
            Deactivate();
        }

        private void Activate()
        {
            if (active)
                return;
            active = true;

            SetupFileWatcher();

            EditorUtility.ClearProgressBar();

            currentMoleculeFilepath = EditorPrefs.GetString("visualiser.prefab.file");
            if (currentMoleculeFilepath != null && File.Exists(currentMoleculeFilepath))
            {
                Debug.Log(currentMoleculeFilepath);
                LoadFile(() => LoadFile(currentMoleculeFilepath));
            }

            RefreshFromSelection();
        }

        private void Deactivate()
        {
            if (!active)
                return;
            active = false;

            RemoveFileWatcher();

            EditorUtility.ClearProgressBar();
            if (currentVisualiser != null)
                DestroyImmediate(currentVisualiser);

            currentVisualiser = null;
            currentMolecule = null;
            jsonTempFileName = null;
        }

        private bool active = false;

        private string structureId = "";

        private const string VisualiserSourceSelection = "Selection";
        private const string VisualiserSourceCustom = "Custom";

        private static readonly string[] VisualiserSources =
        {
            VisualiserSourceSelection, VisualiserSourceCustom
        };


        private const string MoleculeSourceRcsb = "RCSB";
        private const string MoleculeSourceUrl = "URL";
        private const string MoleculeSourceLocal = "File";

        private static readonly string[] MoleculeSources =
        {
            MoleculeSourceRcsb, MoleculeSourceUrl, MoleculeSourceLocal
        };

        private int moleculeSource = 0;
        private int visualiserSource = 0;

        internal GUIStyle GetStyle(string styleName)
        {
            GUIStyle guiStyle = GUI.skin.FindStyle(styleName) ?? EditorGUIUtility
                                                                 .GetBuiltinSkin(
                                                                     EditorSkin.Inspector)
                                                                 .FindStyle(styleName);
            if (guiStyle == null)
            {
                Debug.LogError((object) ("Missing built-in guistyle " + styleName));
                return null;
            }

            return guiStyle;
        }

        private string jsonTempFileName;

        public override void OnToolGUI(EditorWindow window)
        {
            if (!EditorTools.IsActiveTool(this))
            {
                Deactivate();
                return;
            }

            Activate();

            Handles.BeginGUI();

            frameBoxStyle = GetStyle("FrameBox");

            GUILayout.BeginArea(
                new Rect(8, 8, 256, window.position.height),
                GUIContent.none);

            GUILayout.BeginVertical(frameBoxStyle);

            GUILayout.BeginVertical(EditorStyles.helpBox);
            moleculeSource =
                GUILayout.Toolbar(moleculeSource, MoleculeSources, EditorStyles.toolbarButton);
            GUILayout.EndVertical();

            switch (MoleculeSources[moleculeSource])
            {
                case MoleculeSourceRcsb:
                    EditorGUILayout.BeginHorizontal();
                    structureId = EditorGUILayout.TextField(structureId, EditorStyles.miniTextField,
                                                            GUILayout.MaxWidth(40));
                    if (GUILayout.Button("Load", EditorStyles.miniButton))
                    {
                        LoadFile(() => LoadCifFileFromRcsb(structureId));
                    }

                    EditorGUILayout.EndHorizontal();
                    break;
                case MoleculeSourceLocal:
                    break;
            }

            GUILayout.Space(6);

            GUILayout.BeginVertical(EditorStyles.helpBox);
            visualiserSource =
                GUILayout.Toolbar(visualiserSource, VisualiserSources, EditorStyles.toolbarButton);
            GUILayout.EndVertical();

            switch (VisualiserSources[visualiserSource])
            {
                case VisualiserSourceCustom:
                    EditorGUILayout.TextArea(customVisualiserJson);

                    if (GUILayout.Button("Edit File"))
                    {
                        InternalEditorUtility.OpenFileAtLineExternal(
                            jsonTempFileName, 1);
                    }

                    if (GUILayout.Button("Generate Visualiser"))
                    {
                        if (Deserialize(customVisualiserJson) is Dictionary<string, object> content)
                        {
                            SetVisualiserFromDictionary(content);
                        }
                    }

                    break;
            }

            GUILayout.EndVertical();

            GUILayout.EndArea();

            var rect = new Rect(Vector2.zero, window.position.size);
            var x = 0.5f * (rect.xMin + rect.xMax);
            var y = 0.5f * (rect.yMin + rect.yMax);
            var w = 176f;
            var h = 38f;

            if (currentVisualiser == null)
            {
                EditorGUI.HelpBox(new Rect(x - w / 2f, y - h / 2f, w, h),
                                  "Select a Visualiser Prefab", MessageType.Error);
            }
            else if (currentMolecule?.CurrentFrame?.ParticleCount == 0)
            {
                EditorGUI.HelpBox(new Rect(x - w / 2f, y - h / 2f, w, h),
                                  "Load a Molecule", MessageType.Error);
            }

            Handles.EndGUI();
        }

        private void RefreshFromSelection()
        {
            var obj = Selection.activeObject;
            if (obj == null)
                return;

            if (obj == currentVisualiser)
                return;

            Dictionary<string, object> content = null;

            if (obj is VisualisationSubgraph subgraph)
            {
                var path = AssetDatabase.GetAssetPath(obj);
                if (path == null)
                    return;

                if (obj.name == "color pulser")
                {
                    content = new Dictionary<string, object>()
                    {
                        ["color"] = "cpk"
                    };
                }
                else if (path.Contains("Subgraph/Color"))
                {
                    content = new Dictionary<string, object>()
                    {
                        ["color"] = obj.name
                    };
                }
                else if (path.Contains("Subgraph/Render"))
                {
                    content = new Dictionary<string, object>()
                    {
                        ["color"] = "element",
                        ["render"] = obj.name
                    };
                    if (subgraph.GetInputNode<int[]>(VisualiserFactory.SequenceLengthsKey) != null)
                    {
                        content["color"] = "residue name";
                        content["sequence"] = "polypeptide";
                    }
                }
            }
            else if (obj is TextAsset text && AssetDatabase.GetAssetPath(obj).Contains("Prefab"))
            {
                content = Deserialize(text.text) as Dictionary<string, object>;
            }

            if (content != null)
            {
                SetVisualiserFromDictionary(content);
            }
        }

        private void SetVisualiserFromDictionary(Dictionary<string, object> content)
        {
            if (currentVisualiser != null)
                DestroyImmediate(currentVisualiser);

            currentVisualiser = VisualiserFactoryNew.ConstructVisualiser(content);
            //currentVisualiser.hideFlags = HideFlags.DontSaveInEditor | HideFlags.HideInHierarchy;
            if (currentMolecule != null)
                UpdateRenderer(currentMolecule);
        }

        public static object Deserialize(string json)
        {
            return ToObject(JToken.Parse(json));
        }

        private static object ToObject(JToken token)
        {
            switch (token.Type)
            {
                case JTokenType.Object:
                    return token.Children<JProperty>()
                                .ToDictionary(prop => prop.Name,
                                              prop => ToObject(prop.Value));

                case JTokenType.Array:
                    return token.Select(ToObject).ToList();

                default:
                    return ((JValue) token).Value;
            }
        }

        private string currentMoleculeFilepath;
        private FrameSource currentMolecule;

        /// <summary>
        /// Show a progress message.
        /// </summary>
        /// <remarks>
        /// If called from another thread, <paramref name="threadSafe" /> should be false.
        /// This will queue showing the update until the next time the UI is refreshed, as
        /// <see cref="EditorUtility.DisplayProgressBar" /> can only be called from the
        /// main thread.
        /// </remarks>
        public static void ShowProgressMessage(string message, bool threadSafe)
        {
            if (threadSafe)
            {
                EditorUtility.DisplayProgressBar("Visualisation Preview", message, 0.5f);
                queuedProgressMessage = null;
            }
            else
            {
                queuedProgressMessage = message;
            }
        }

        /// <summary>
        /// Hide the progress bar
        /// </summary>
        private static void HideProgressMessage()
        {
            queuedProgressMessage = null;
            EditorUtility.ClearProgressBar();
        }

        private static string queuedProgressMessage;

        public async void LoadFile(Func<Task<Frame.Frame>> getFrame)
        {
            try
            {
                var frame = await getFrame();

                frame.RecenterAroundOrigin();
                currentMolecule = new FrameSource(frame);

                UpdateRenderer(currentMolecule);

                EditorPrefs.SetString("visualiser.prefab.file", currentMoleculeFilepath);
            }
            catch (Exception e)
            {
            }
            finally
            {
                HideProgressMessage();
            }
        }

        private void Update()
        {
            if (!string.IsNullOrEmpty(queuedProgressMessage))
            {
                ShowProgressMessage(queuedProgressMessage, true);
            }
        }

        private class FrameSource : ITrajectorySnapshot
        {
            public FrameSource(Frame.Frame frame)
            {
                CurrentFrame = frame;
            }

            public Frame.Frame CurrentFrame { get; }
            public event FrameChanged FrameChanged;
        }

        private GameObject currentVisualiser;

        private IntArrayProperty highlightedParticles = new IntArrayProperty();

        private FrameAdaptorNode frameAdaptor;

        /// <summary>
        /// Update the prefab with the provided molecule
        /// </summary>
        private void UpdateRenderer(ITrajectorySnapshot molecule)
        {
            if (currentVisualiser == null)
                return;

            var adaptor = currentVisualiser
                          .GetVisualisationNodes<ParentedAdaptorNode>()
                          .FirstOrDefault(a => !a.ParentAdaptor.HasValue);
            if (adaptor != null)
            {
                frameAdaptor = new FrameAdaptorNode
                {
                    FrameSource = molecule
                };

                frameAdaptor.AddOverrideProperty<int[]>("highlighted.particles").LinkedProperty =
                    highlightedParticles;

                adaptor.ParentAdaptor.Value = frameAdaptor;
                adaptor.Refresh();
            }

            currentVisualiser.gameObject.SendMessage("Update");
        }

        public async Task<Frame.Frame> LoadFile(string filename)
        {
            currentMoleculeFilepath = filename;
            ShowProgressMessage("Loading File", true);

            var dictionary = ChemicalComponentDictionary.Instance;

            return await Task.Run(() => ImportCifFile(filename, dictionary));
        }

        private bool isHighlighted = false;

        public bool IsHighlighted
        {
            get => isHighlighted;
            set
            {
                if (isHighlighted == value)
                    return;
                isHighlighted = value;
                if (isHighlighted)
                    highlightedParticles.Value = Enumerable.Range(0, 100).ToArray();
                else
                    highlightedParticles.UndefineValue();
            }
        }

        public FrameAdaptorNode FrameAdaptor => frameAdaptor;


        private static async Task<Frame.Frame> ImportCifFile(
            string filename,
            ChemicalComponentDictionary dictionary)
        {
            using (var file = File.OpenRead(filename))
            using (var reader = new StreamReader(file))
            {
                if (Path.GetExtension(filename).Contains("cif"))
                {
                    if (Path.GetFileNameWithoutExtension(filename).Length == 3)
                        return CifChemicalComponentImport.Import(reader, new Progress());
                    return CifImport.Import(reader, dictionary, new Progress());
                }
            }

            return null;
        }

        private class Progress : IProgress<string>
        {
            public void Report(string value)
            {
                ShowProgressMessage(value, false);
            }
        }


        public async Task<Frame.Frame> LoadCifFileFromRcsb(string id)
        {
            if (string.IsNullOrEmpty(id))
                return null;

            var moleculeFilepath = $"{Application.temporaryCachePath}/{id}.cif";

            if (File.Exists(moleculeFilepath))
            {
                return await LoadFile(moleculeFilepath);
            }
            else
            {
                var client = new WebClient();
                client.CancelAsync();
                ShowProgressMessage("Downloading file", true);

                await client.DownloadFileTaskAsync(
                    new Uri($"https://files.rcsb.org/download/{id}.cif"),
                    moleculeFilepath);
                return await LoadFile(moleculeFilepath);
            }
        }
    }
}