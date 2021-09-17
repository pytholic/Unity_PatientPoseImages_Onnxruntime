// https://bitbucket.org/alkee/aus
using JeremyAnsel.Media.WavefrontObj; // https://github.com/JeremyAnsel/JeremyAnsel.Media.WavefrontObj
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.Events;

namespace aus.Geometry
{
    public class WavefrontObjMesh
        : MonoBehaviour
    {
        [Header("Initial load settings")]
        [SerializeField]
        private string sourceFilePath;
        [Tooltip("flip x coordination for left hand system")]
        [SerializeField]
        private bool flipXcoordination = true;

        [Header("Defaults")]
        [Tooltip("Standard shader for none value")]
        [SerializeField]
        private Shader diffuseShader;

        [Header("Event")]
        public UnityEvent<WavefrontObjMesh> onLoad;

        public string SourceFilePath { get; private set; }
        public MeshFilter GetMeshFilter()
        {
            return GetComponentInChildren<MeshFilter>();
        }

        public void Clear()
        {
            foreach (Transform t in transform)
            {
                Destroy(t.gameObject);
            }
            SourceFilePath = null;
        }

        public async Task<IEnumerable<GameObject>> LoadAsync(string sourceFilePath, bool flipXcoordination = true)
        {
            Clear();

            // TODO: objFile.MaterialLibraries support
            var defaultMaterial = new Material(diffuseShader);
            ObjFile objFile = null;

            await Task.Run(() =>
            { // material 이나 transform 을 다루는 함수는 main thread 에서 작동해야해서 Task 로 넣을 수 없다.
                Debug.Log($"loading mesh from {sourceFilePath}");
                objFile = ObjFile.FromFile(sourceFilePath); // ObjFile 이 async interface 를 지원한다면 좋았을텐데..
            });

            // TODO: group mesh/sub mesh 지원
            var go = CreateMeshObject(objFile, defaultMaterial, transform, flipXcoordination);
            if (go) SourceFilePath = sourceFilePath;
            onLoad?.Invoke(this);
            return new GameObject[] { go };
        }

        private static GameObject CreateMeshObject(ObjFile source, Material defaultMaterial, Transform parent, bool flipXcoordination)
        {
            // prepare data
            var dirX = flipXcoordination ? -1 : 1;
            var vertices = source.Vertices.Select(v => new Vector3(v.Position.X * dirX, v.Position.Y, v.Position.Z)).ToArray();
            var normals = source.VertexNormals?.Select(n => new Vector3(n.X * dirX, n.Y, n.Z)).ToArray();
            var faces = new List<int>();
            foreach (var f in source.Faces)
            {
                // face 의 flipping 은 face index 순서를 바꾸는 것. : https://youtu.be/eJEpeUH1EMg?t=196
                for (var i = flipXcoordination ? 2 : 0; i >= 0 && i < 3; i += dirX)
                    faces.Add(f.Vertices[i].Vertex - 1); // wavefront .obj 의 index 는 1 부터 시작.
            }

            var obj = new GameObject("obj mesh");
            obj.transform.parent = parent;

            // mesh filter setup
            var mf = obj.AddComponent<MeshFilter>();
            var mesh = mf.mesh;
            mesh.vertices = vertices;
            mesh.triangles = faces.ToArray();
            if (normals.Length == vertices.Length)
            {
                mesh.normals = normals;
            }
            else
            {
                Debug.LogWarning($"normal count dismatched. vertices : {vertices.Length}, normals : {normals.Length}. recalculating ...");
                mesh.RecalculateNormals();
            }
            Debug.Log($"mesh created at {mesh.bounds:F4}, vertices: {vertices.Length}, faces: {faces.Count / 3}");

            // mesh renderer setup
            var mr = obj.AddComponent<MeshRenderer>();
            mr.material = defaultMaterial;

            return obj;
        }

        #region Unity message handlers

        private void Awake()
        {
            if (!diffuseShader)
            { // initializer 에서 Shader.Find 를 사용할 수 없다.
                // 참조가 없어 shader 가 resource 에 포함되지 않을 경우 build 된 실행환경에서 오류발생한다.
                // project 기본설정으로 포함되어있는 shader (Project settings > Graphics > Always Included Shaders
                diffuseShader = Shader.Find("Legacy Shaders/Diffuse");
                Debug.Assert(diffuseShader);
            }
        }

        private async void Start()
        {
            if (string.IsNullOrEmpty(sourceFilePath)) return;
            await LoadAsync(sourceFilePath, flipXcoordination);
        }

        #endregion Unity message handlers
    }
}