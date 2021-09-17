// https://bitbucket.org/alkee/aus

using aus.Extension;
using System.Collections;
using System.IO;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.UI;

namespace aus.Ui
{
    [RequireComponent(typeof(Image))]
    [DisallowMultipleComponent]
    public class WwwImage : MonoBehaviour
    {
        public string ImageUrl;
        public bool NoCache = false;
        public ResizeOptionEnum ResizeOption = ResizeOptionEnum.FIT_TO_SIZE;

        public enum ResizeOptionEnum
        {
            // texture fit to this transform
            FIT_TO_SIZE,
            PRESERVE_RATIO,
            NATIVE_SIZE,
        }

        [Header("image preset")]
        public Texture2D InitialImage;
        public Texture2D LoadingImage;
        public Texture2D FailImage;

        public void SetImageUrl(string url, bool cache = true)
        {
            NoCache = !cache;
            ImageUrl = url;
            SetLocalPath(ImageUrl);
            Refresh();
        }

        public void SetImageCache(string path)
        {
            NoCache = false;
            ImageUrl = "file://" + path;
            localFilePath = path;
            Refresh();
        }

        public bool DeleteCacheFile()
        {
            if (File.Exists(localFilePath) == false) return false;
            File.Delete(localFilePath);
            return true;
        }

        public void Refresh()
        {
            // TODO: loading, no-source or fail image support
            StartCoroutine(BeginDownload());
        }

        void Awake()
        {
            target = GetComponent<Image>();
            if (string.IsNullOrEmpty(ImageUrl) || enabled == false) return;
            SetLocalPath(ImageUrl);
        }

        void Start()
        {
            ApplyTexture(InitialImage);
            Refresh();
        }

        private Image target;
        private string localFilePath;

        private IEnumerator BeginDownload()
        {
            if (string.IsNullOrEmpty(ImageUrl)) yield break; // invalid url

            string localPath = localFilePath;

            if (File.Exists(localPath) && NoCache == false)
            {
                yield return ApplySprite();
                yield break;
            }

            ApplyTexture(LoadingImage);
            // TODO: download into temporary file to handle errors on the file
            var www = UnityWebRequest.Get(ImageUrl);
#if UNITY_2017_2_OR_NEWER
            yield return www.SendWebRequest();
#else
            yield return www.Send();
#endif
            if (string.IsNullOrEmpty(www.error) == false)
            { // failed to download
                Debug.LogError(www.error + " : " + ImageUrl);
                ApplyTexture(FailImage);
                yield break;
            }
            try
            {
                File.WriteAllBytes(localPath, www.downloadHandler.data);
            }
            catch (System.Exception e)
            {
                Debug.LogError("WwwImage failed : " + e.ToString());
                ApplyTexture(FailImage);
                try
                {
                    File.Delete(localPath);
                }
                catch (System.Exception)
                {
                }
            }
            yield return ApplySprite();
        }

        private IEnumerator ApplySprite()
        {
            string localPath = localFilePath;

#if UNITY_2017_2_OR_NEWER
            var www = UnityWebRequestTexture.GetTexture("file://" + localPath); // easier way to get source image size
            yield return www.SendWebRequest();
#else
            var www = UnityWebRequest.Get("file://" + localPath);
            yield return www.Send();
#endif
            if (string.IsNullOrEmpty(www.error) == false)
            { // failed
                Debug.LogErrorFormat("error to apply file : {0}", localPath);
                ApplyTexture(FailImage);
            }
            else
            {
                var texture = DownloadHandlerTexture.GetContent(www);
                target.sprite = Sprite.Create(texture, new Rect(0, 0, texture.width, texture.height), Vector2.one * 0.5f);
                if (ResizeOption == ResizeOptionEnum.NATIVE_SIZE)
                {
                    target.SetNativeSize();
                }
                else if (ResizeOption == ResizeOptionEnum.PRESERVE_RATIO)
                {
                    target.preserveAspect = true;
                }
                // UNDONE: width height ratio, offset, fitting
            }

        }

        private void ApplyTexture(Texture2D texture)
        {
            if (texture == null) return;
            target.sprite = Sprite.Create(texture, new Rect(0, 0, texture.width, texture.height), Vector2.one * 0.5f);
        }

        private void SetLocalPath(string url)
        {
            localFilePath = Path.Combine(Application.persistentDataPath, url.Trim().Sha1());
        }
    }
}
