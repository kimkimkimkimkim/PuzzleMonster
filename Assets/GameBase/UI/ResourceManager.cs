using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.IO.IsolatedStorage;
using System.Linq;
using UniRx;
using UnityEngine;
using GameBase;

namespace GameBase
{
    public class AssetBundleInfo
    {
        public string WithExtensionPath;
        public string bundlePath;
        public AssetBundleRequest AssetBundleRequest;
        public List<string> dependencies;
    }

    public class ResourceManager : SingletonMonoBehaviour<ResourceManager>
    {
#if UNITY_EDITOR

        #region Editor DebugFactor

        public bool isUseAssetBundle = true;

        #endregion Editor DebugFactor

#endif

        #region Monobehavior Func

        /// <summary>
        /// Awake
        /// </summary>
        /*
        protected override void Awake()
        {
            base.Awake();
            if (base.IsDestroyed()) return;

            DontDestroyOnLoad(gameObject);
        }
        */

        #endregion Monobehavior Func

        /// <summary>
        /// has Manager been initialized?
        /// </summary>
        public bool isInited
        {
            get
            {
                return _isInited;
            }
        }

        private bool _isInited = false;

        private Dictionary<string, AssetBundleInfo> _assetbundleHolder = new Dictionary<string, AssetBundleInfo>();
        private Dictionary<string, string> _assetHolder = new Dictionary<string, string>();

        private Dictionary<string, AssetBundle> loadedBundleHolder = new Dictionary<string, AssetBundle>();

        //public LocalCatalog Catalog { get; private set; }

        public const string ASSET_PATH_PREFIX = "Assets/Contents/Resources";

        /// <summary>
        /// 差分ダウンロード確認ダイアログを表示させるための、差分コンテンツのサイズ（Mbyte）
        /// </summary>
        private const int DISPLAY_NEED_DOWNLOAD_DIALOG_CONTENTS_SIZE = 100;

        /*
        / Addressable採用により使用しなくなった
        public IObservable<Unit> PrepareAndUpdateLocalAssets(Action<int, int> OnUpdateProgress = null, Action<int, int> OnUpdateTextProgress = null) {//ローカルアセットの準備とアップデート
    #if UNITY_EDITOR
            if (!isUseAssetBundle) {
                KoiniwaLogger.Log("AssetBundleを使わない環境なのでダウンロード処理をスキップ");
                return Observable.ReturnUnit();
            }
    #endif

            return LoadLocalCatalog()
                .DoOnCompleted(() => KoiniwaLogger.Log("Loaded Local AssetBundle Catalog:Asset"))
                .SelectMany(_ => LoadRemoteCatalog())
                .ObserveOnMainThread()
                .SelectMany(remoteBytes => {
                    return Observable.ReturnUnit()
                        .SelectMany(_ => SharpZipUtil.UnzipObservable<ContentAssetCatalog>(remoteBytes))
                        .ObserveOnMainThread()
                        .DoOnCompleted(() => KoiniwaLogger.Log("Loaded Remote AssetBundle Catalog:Asset"))
                        .SelectMany(contentAssetCatalog => VersionCheckAndDownload(Catalog, contentAssetCatalog, remoteBytes, OnUpdateProgress, OnUpdateTextProgress))// 取得したリモートアセットバンドルカタログの中身とローカルのアセットバンドルを比較してダウンロード処理
                        .DoOnCompleted(() => KoiniwaLogger.Log("Complete VersionCheck And Download:Asset"));
                })
                .AsUnitObservable();
        }

        private enum CheckType : byte {
            None,
            FirstDownload,
            FirstDownloadAgain,
            VersionUp
        }

        private IObservable<FileInfo> VersionCheckAndDownload(LocalCatalog localCatalog, ContentAssetCatalog remoteAssetCatalog, byte[] remoteBytes, Action<int, int> OnUpdateProgress = null, Action<int, int> OnUpdateTextProgress = null) {
            var checkType = CheckType.None;
            if (Catalog.Hash == null) {
                var haveTriedFirstDownload = PlayerPrefsUtil.System.GetHaveTriedFirstDownload();
                checkType = haveTriedFirstDownload ? CheckType.FirstDownloadAgain : CheckType.FirstDownload;
                PlayerPrefsUtil.System.SetHaveTriedFirstDownload(true);
            } else {
                var isLatestVersion = localCatalog.CompareHashSummary(remoteAssetCatalog.hashSummary);
                checkType = isLatestVersion ? CheckType.None : CheckType.VersionUp;
            }

            KoiniwaLogger.Log("CheckType is " + checkType.ToString() + "Asset");
            IObservable<Unit> stream = Observable.ReturnUnit();

            switch (checkType) {
                case CheckType.None:
                    break;

                case CheckType.FirstDownload:
                    Catalog = new LocalCatalog(remoteAssetCatalog);
                    stream = Catalog.ChangeAllStateDownload()
                             .DoOnCompleted(() => KoiniwaLogger.Log("Update State : All Download:Asset"));
                    break;

                case CheckType.FirstDownloadAgain:
                    Catalog = new LocalCatalog(remoteAssetCatalog);
                    stream = Catalog.ReflectLocalState()
                             .DoOnCompleted(() => KoiniwaLogger.Log("Update ReflectLocalState:Asset"));
                    break;

                case CheckType.VersionUp:
                    stream = Catalog.ReflectUpdate(remoteAssetCatalog);
                    break;

                default:
                    break;
            }

            return stream.ObserveOnMainThread()
                .SelectMany(_ => DisplayNeedDownloadDialog(checkType))
                .SelectMany(_ => {
                    DeleteObsoleteFiles();
                    return DownloadRequiredFiles(OnUpdateProgress, OnUpdateTextProgress);
                })
            .Do(file => KoiniwaLogger.Log("Downloaded File : " + file.FullName + "Asset"))
            .LastOrDefault()
            .SelectMany(_ => {
                Catalog = new LocalCatalog(remoteAssetCatalog);
                return LocalFiles.WriteFile(remoteBytes, LocalFiles.CATALOGASSETBUNDLE_PATH);
            });
        }

        private IObservable<Unit> DisplayNeedDownloadDialog(CheckType checkType) {
            var needDownloadEntry = Catalog.GetAllEntryOfState(ContentAssetLocalState.NeedDownload);
            long downloadSize = 0;
            var needDownTotalFilesCount = needDownloadEntry.Count;
            for (int i = 0; i < needDownTotalFilesCount; i++) {
                var entry = needDownloadEntry[i];
                if (entry == null)
                    continue;

                downloadSize += entry.Entry.fileSize;
            }

            return Observable.Create<Unit>(observer => {
                var mbyte = (downloadSize / 1024f) / 1024f;

                //100MB以上の場合は確認ダイアログを表示
                if (mbyte >= DISPLAY_NEED_DOWNLOAD_DIALOG_CONTENTS_SIZE && checkType != CheckType.FirstDownload && checkType != CheckType.FirstDownloadAgain) {
                    var title = ApplicationContext.LocaledTextResource.GetString("[CONFIRM_DOWNLOAD_TITLE]");
                    var content = ApplicationContext.LocaledTextResource.GetString("[START_DOWNLOAD_DIALOG_CONTENT]", mbyte.ToString("0"));
                } else {
                    observer.OnNext(Unit.Default);
                    observer.OnCompleted();
                }

                return Disposable.Empty;
            });
        }

        private IObservable<FileInfo> DownloadRequiredFiles(System.Action<int, int> updateProgress = null, System.Action<int, int> updateTextProgress = null) {
            var needDownloadEntry = Catalog.GetAllEntryOfState(ContentAssetLocalState.NeedDownload);

            var needDownTotalFilesCount = needDownloadEntry.Count;
            var currentCount = 1;
            var progressNotifier = new ScheduledNotifier<float>();
            progressNotifier.Subscribe(x => {
                if (updateProgress != null)
                    updateProgress((int)x, 100);
            });

            return needDownloadEntry.ToObservable()
                .Select(entry => LocalFiles.Download(entry, ServerConfig.AssetBundleLocationRoot, progress: progressNotifier))
                .Concat()
                .Do(_ => updateTextProgress(currentCount++, needDownTotalFilesCount));
        }


        public void DeleteObsoleteFiles() {
            var needRemovalList = Catalog.GetAllEntryOfState(ContentAssetLocalState.NeedRemoval);
            DeleteFiles(needRemovalList);
            Catalog.RemoveObsoleteEntry();
            KoiniwaLogger.Log(needRemovalList.Count + "個アセットを削除:AssetBundle");
        }

        private void DeleteFiles(List<LocalEntry> filesToBeDeleted) {
            filesToBeDeleted.ForEach(LocalFiles.Delete);
        }

        public IObservable<byte[]> LoadRemoteCatalog() {
            ServerConfig.AssetBundleCatalogURL = ServerConfig.AssetBundleLocationRoot.TrimEnd('/') + '/' + APIConnection.GetPlatformName() + "/" + APIConnection.GetPlatformName() + ".mct";
            KoiniwaLogger.Log("Download RemoteCatalog. path : " + ServerConfig.AssetBundleCatalogURL + ", rootPath : " + ServerConfig.AssetBundleLocationRoot + ", sharedPath : " + ServerConfig.SharedAssetLocationRoot);
            return LocalFiles.LoadFileFromRemoteAs<byte[]>(ServerConfig.AssetBundleCatalogURL);
        }

        public IObservable<LocalCatalog> LoadLocalCatalog() {
            return LocalFiles.Load(LocalFiles.CATALOGASSETBUNDLE_PATH)
                .Select(data => Catalog = new LocalCatalog(data, true))
                .Catch((DirectoryNotFoundException e) => InitLocalCatalog())
                .Catch((IsolatedStorageException e) => InitLocalCatalog())
                .Catch((FileNotFoundException e) => InitLocalCatalog())
                .Catch((EndOfStreamException e) => InitLocalCatalog());
        }

        private IObservable<LocalCatalog> InitLocalCatalog() {
            return Observable.Create<LocalCatalog>(obs => { obs.OnNext(Catalog = new LocalCatalog()); obs.OnCompleted(); return Disposable.Empty; });
        }

        /// <summary>
        /// Call this Function first before use ResourceManager
        /// </summary>
        /// <returns></returns>
        public void Init() {
            if (!_isInited) {
                _isInited = true;

    #if UNITY_EDITOR
                if (!isUseAssetBundle)
                    InitAssetDataBaseHolder();
                else {
                    InitAssetBundleHolder(GetAllAssetNames());
                }
    #else

            InitAssetBundleHolder(GetAllAssetNames());
    #endif
            }
        }



        public void ReloadAssetBundleOnChanged() {
    #if UNITY_EDITOR
            if (!isUseAssetBundle) {
                KoiniwaLogger.Log("AssetBundleを使わない環境なのでキャッシュのリロード処理をスキップ");
                return;
            }
    #endif
            _assetbundleHolder.Clear();
            UnloadBundle(false);
            InitAssetBundleHolder(GetAllAssetNames());
        }


        public AllAssetBundleNames GetAllAssetNames() {
            FileInfo localFile = new FileInfo(Path.Combine(LocalFiles.PersistentAssetsPath, "AssetBundle" + "/" + APIConnection.GetPlatformName() + "/" + APIConnection.GetPlatformName() + ".kch"));
            var bytes = LocalFiles.LoadDirect(localFile);
            var allAssetBundleNames = SharpZipUtil.Unzip<AllAssetBundleNames>(bytes);
            return allAssetBundleNames;
        }

        private void InitAssetBundleHolder(AllAssetBundleNames allAssetNames) {
            string persitenAssetsPath = LocalFiles.PersistentAssetsPath.Replace("\\", "/");

            foreach (var entity in allAssetNames.EntryMap) {
                foreach (var assetNameInfo in entity.GetAllAssetNames) {
                    if (_assetbundleHolder.ContainsKey(assetNameInfo.LoadPath)) {
                        if (assetNameInfo.AssetFullPath.EndsWith(".prefab")) {
                            _assetbundleHolder[assetNameInfo.LoadPath].WithExtensionPath = assetNameInfo.AssetFullPath;
                        }
                        continue;
                    }
                    _assetbundleHolder.Add(assetNameInfo.LoadPath, new AssetBundleInfo() { bundlePath = persitenAssetsPath + "/" + entity.RelativePath, WithExtensionPath = assetNameInfo.AssetFullPath, dependencies = entity.GetDependencyLoadPaths });
                }
                foreach (var directoryName in entity.LoadAllPaths) {
                    if (_assetbundleHolder.ContainsKey(directoryName))
                        continue;
                    _assetbundleHolder.Add(directoryName, new AssetBundleInfo() { bundlePath = persitenAssetsPath + "/" + entity.RelativePath, WithExtensionPath = entity.RelativePath, dependencies = entity.GetDependencyLoadPaths });
                }
            }

            KoiniwaLogger.Log("Complete Cache AssetBundle");
        }

        public void InitAssetDataBaseHolder() {
            var files = System.Array.FindAll(Directory.GetFiles(ASSET_PATH_PREFIX, "*", SearchOption.AllDirectories), x => !x.EndsWith(".meta"));
            var directories = System.Array.FindAll(Directory.GetDirectories(ASSET_PATH_PREFIX, "*", SearchOption.AllDirectories), x => !x.EndsWith(".meta"));

            foreach (string path in files) {
                int num = path.IndexOf(ASSET_PATH_PREFIX);
                var replacePath = path.Replace("\\", "/");
                var key = GetPathWithoutExtension(replacePath);
                key = key.Remove(0, num + ASSET_PATH_PREFIX.Length + 1).ToLower();
                if (_assetHolder.ContainsKey(key)) {
                    if (Path.GetExtension(path) == ".prefab") {
                        _assetHolder[key] = replacePath;
                    }
                    continue;
                }
                _assetHolder.Add(key, replacePath);
            }
            foreach (string path in directories) {
                int num = path.IndexOf(ASSET_PATH_PREFIX);
                var replacePath = path.Replace("\\", "/");
                var key = replacePath.Remove(0, num + ASSET_PATH_PREFIX.Length + 1).ToLower();
                if (_assetHolder.ContainsKey(key))
                    continue;

                _assetHolder.Add(key, replacePath);
            }
            KoiniwaLogger.Log("Complete Cache AssetBundle");
        }
        */

        /// <summary>
        ///
        /// </summary>
        public void UnloadBundle(bool unloadAllLoadedAsset = true)
        {
            foreach (AssetBundle bundle in loadedBundleHolder.Values)
            {
                if (bundle != null)
                    bundle.Unload(unloadAllLoadedAsset);
            }

            loadedBundleHolder.Clear();
        }

        public string GetPathWithoutExtension(string path)
        {
            var extension = Path.GetExtension(path);
            if (string.IsNullOrEmpty(extension))
            {
                return path;
            }
            return path.Replace(extension, string.Empty);
        }

        private AssetBundle GetAssetBundle(AssetBundleInfo info)
        {
            AssetBundle assetbundle = null;

            if (loadedBundleHolder.TryGetValue(info.bundlePath, out assetbundle) == false)
            {
                assetbundle = AssetBundle.LoadFromFile(info.bundlePath);
                loadedBundleHolder.Add(info.bundlePath, assetbundle);

                if (info.dependencies != null && info.dependencies.Count > 0)
                    LoadDependencies(info.dependencies);
            }

            return assetbundle;
        }

        private void LoadDependencies(List<string> path)
        {
            /*
            string LOCAL_RELATIVE_PATH_PREFIX = "AssetBundle" + "/" + APIConnection.GetPlatformName();

            string persitenAssetsPath = LocalFiles.PersistentAssetsPath.Replace("\\", "/");
            for (int i = 0; i < path.Count; i++)
            {
                var bundlePath = string.Format("{0}/{1}/{2}", persitenAssetsPath, LOCAL_RELATIVE_PATH_PREFIX, path[i]);
                AssetBundle assetbundle = null;

                if (loadedBundleHolder.TryGetValue(bundlePath, out assetbundle) == false)
                {
                    assetbundle = AssetBundle.LoadFromFile(bundlePath);
                    loadedBundleHolder.Add(bundlePath, assetbundle);
                }
            }
            */
        }


        /// <summary>
        /// Try load asset in aseetbundle before load it in Resources folder.
        /// </summary>
        /// <typeparam name="T">UnityObject</typeparam>
        /// <param name="path">Path Without Extension</param>
        /// <returns>typeof T Object</returns>
        public T LoadAsset<T>(string path) where T : UnityEngine.Object
        {
            if (string.IsNullOrEmpty(path))
            {
                Debug.LogError("ResourceManager Error :: LoadAsset - Invalid Path: " + path);
                return default(T);
            }
            string replacePath = path.Replace("\\", "/");
            string lowerPath = replacePath.ToLower();
            T resourceAsset = null;

#if UNITY_EDITOR
            if (!isUseAssetBundle)
            {
                if (!_assetHolder.ContainsKey(lowerPath))
                {
                    resourceAsset = Resources.Load<T>(path);
                }
                else
                {
                    resourceAsset = UnityEditor.AssetDatabase.LoadAssetAtPath<T>(_assetHolder[lowerPath]);
                }
            }
            else
            {
#endif
                if (!_assetbundleHolder.ContainsKey(lowerPath))
                {
                    resourceAsset = Resources.Load<T>(path);
                }
                else
                {
                    AssetBundle bundle = GetAssetBundle(_assetbundleHolder[lowerPath]);

                    if (bundle != null)
                        resourceAsset = bundle.LoadAsset<T>(_assetbundleHolder[lowerPath].WithExtensionPath);
                }
#if UNITY_EDITOR
            }
#endif
            if (resourceAsset == null)
                Debug.LogWarning("Fail LoadAsset<T> Path : " + lowerPath);
            return resourceAsset;
        }

        public T[] LoadAllAssets<T>(string path) where T : UnityEngine.Object
        {
            if (string.IsNullOrEmpty(path))
            {
                Debug.LogError("ResourceManager Error :: LoadAsset - Invalid Path");
                return null;
            }

            string replacePath = path.Replace("\\", "/");
            if (replacePath.EndsWith("/"))
                replacePath = replacePath.Remove(replacePath.Length - 1, 1);
            string lowerPath = replacePath.ToLower();
            T[] resourceAsset = null;

#if UNITY_EDITOR
            if (!isUseAssetBundle)
            {
                if (!_assetHolder.ContainsKey(lowerPath))
                {
                    resourceAsset = Resources.LoadAll<T>(path);
                }
                else
                {
                    List<T> list = new List<T>();
                    string[] filePaths = Directory.GetFiles(_assetHolder[lowerPath]);
                    for (int i = 0; i < filePaths.Length; i++)
                    {
                        var temp = UnityEditor.AssetDatabase.LoadAssetAtPath<T>(filePaths[i]);
                        if (temp != null)
                            list.Add(temp);
                    }
                    resourceAsset = list.ToArray();
                }
            }
            else
            {
#endif
                if (!_assetbundleHolder.ContainsKey(lowerPath))
                {
                    resourceAsset = Resources.LoadAll<T>(path);
                }
                else
                {
                    AssetBundle bundle = GetAssetBundle(_assetbundleHolder[lowerPath]);

                    if (bundle != null)
                        resourceAsset = bundle.LoadAllAssets<T>();
                }

#if UNITY_EDITOR
            }
#endif

            if (resourceAsset == null || resourceAsset.Length == 0)
                Debug.LogWarning("Fail LoadAllAssets<T> keyPath: " + lowerPath);
            return resourceAsset;
        }

        public IObservable<T> LoadAssetAsyncObservable<T>(string path) where T : UnityEngine.Object
        {
            return Observable.FromCoroutine<T>(observer => AsyncProcesser(observer, path));
        }

        private IEnumerator AsyncProcesser<T>(IObserver<T> observer, string path) where T : UnityEngine.Object
        {
            string replacePath = path.Replace("\\", "/");
            string lowerPath = replacePath.ToLower();
            T temp = null;

#if UNITY_EDITOR
            if (!isUseAssetBundle)
            {
                if (!_assetHolder.ContainsKey(lowerPath))
                {
                    var request = Resources.LoadAsync<T>(path);
                    yield return request;
                    temp = request.asset as T;
                }
                else
                {
                    temp = UnityEditor.AssetDatabase.LoadAssetAtPath<T>(_assetHolder[lowerPath]);
                    yield return null;
                }
            }
            else
            {
#endif
                if (!_assetbundleHolder.ContainsKey(lowerPath))
                {
                    var request = Resources.LoadAsync<T>(path);
                    yield return request;
                    temp = request.asset as T;
                }
                else
                {
                    AssetBundle bundle = GetAssetBundle(_assetbundleHolder[lowerPath]);
                    AssetBundleRequest request = null;
                    if (bundle != null)
                        request = bundle.LoadAssetAsync<T>(_assetbundleHolder[lowerPath].WithExtensionPath);
                    yield return request;
                    temp = request.asset as T;
                }
#if UNITY_EDITOR
            }
#endif

            if (temp == null)
            {
                Debug.LogWarning("Fail LoadAsset<T> Path : " + lowerPath);
            }

            observer.OnNext(temp);
            observer.OnCompleted();
        }

        private void OnApplicationPause(bool pauseStatus)
        {
            if (pauseStatus)
            {
                // ローカルプッシュ通知をスケジュール登録する。
                //KoiniwaPushNotificationManager.Instance.RegisterFarmHarvestTimerLocalPushNotification();
                //KoiniwaPushNotificationManager.Instance.RegisterMinePickTimerLocalPushNotification();
            }
            else
            {
                // 各種登録解除。
                //KoiniwaPushNotificationManager.Instance.UnregisterFarmHarvestTimerLocalPushNotification();
                //KoiniwaPushNotificationManager.Instance.UnregisterMinePickTimerLocalPushNotification();

                // 庭を最新の状態に更新
                //GardenManager.Instance.RefreshBackground();
            }
        }

        /// <summary>
        /// ウェブビューで使用する内製ページのURLを返します。
        /// </summary>
        //public static string GetWebViewUrl(string path) => ServerConfig.WebLocationRoot + path;
    }
}