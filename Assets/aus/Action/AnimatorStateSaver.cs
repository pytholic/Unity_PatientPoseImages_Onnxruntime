// https://bitbucket.org/alkee/aus
using System.Collections.Generic;
using UnityEngine;

namespace aus.Action
{
    // ref: https://forum.unity.com/threads/bool-parameter-does-not-persist-through-deactivation-reactivation-except-if-set-in-editor.287955/
    /// <summary>
    /// Saves Animator state and restores it automatically when the object is activated
    /// Code must explicitly call Reset() to reinit the animator,
    /// and call AnimatorStateSaver.SaveAnimStateRecursive() before deactivating any parent
    /// </summary>
    public class AnimatorStateSaver : MonoBehaviour
    {
        private AnimatorStateInfo _savedAnimatorStateInfo;
        private AnimatorControllerParameter[] _savedParameters;
        private Dictionary<string, System.Object> _savedParameterValues = new Dictionary<string, System.Object>();
        private Animator _animator = null;
        private bool _isSaveAvailable = false;

        public static void SaveAnimStateRecursive(GameObject pGameObject)
        {
            AnimatorStateSaver[] animatorStateSaversArray = pGameObject.GetComponentsInChildren<AnimatorStateSaver>(false);
            for (int i = 0; i < animatorStateSaversArray.Length; i++)
            {
                animatorStateSaversArray[i].SaveAnimState();
            }
        }

        public void Start()
        {
            _animator = GetComponent<Animator>();
            Debug.Assert(_animator != null);
        }


        public void OnEnable()
        {
            if (_isSaveAvailable && _animator != null)
            {
                _LoadParams();
                _animator.Play(_savedAnimatorStateInfo.shortNameHash);
            }
        }


        public void SaveAnimState()
        {
            if (_animator != null)
            {
                _savedAnimatorStateInfo = _animator.GetCurrentAnimatorStateInfo(0);
                _SaveParams();
                _isSaveAvailable = true;
            }
        }


        public void Reset()
        {
            _isSaveAvailable = false;
        }


        private void _SaveParams()
        {
            _savedParameters = _animator.parameters;
            for (int i = 0; i < _savedParameters.Length; i++)
            {
                switch (_savedParameters[i].type)
                {
                    case AnimatorControllerParameterType.Bool:
                        _savedParameterValues[_savedParameters[i].name] = _animator.GetBool(_savedParameters[i].name);
                        break;
                    case AnimatorControllerParameterType.Float:
                        _savedParameterValues[_savedParameters[i].name] = _animator.GetFloat(_savedParameters[i].name);
                        break;
                    case AnimatorControllerParameterType.Int:
                        _savedParameterValues[_savedParameters[i].name] = _animator.GetFloat(_savedParameters[i].name);
                        break;
                    case AnimatorControllerParameterType.Trigger:
                    default:
                        // we don't care
                        break;
                }
            }
        }

        private void _LoadParams()
        {
            for (int i = 0; i < _savedParameters.Length; i++)
            {
                switch (_savedParameters[i].type)
                {
                    case AnimatorControllerParameterType.Bool:
                        _animator.SetBool(_savedParameters[i].name, (bool)_savedParameterValues[_savedParameters[i].name]);
                        break;
                    case AnimatorControllerParameterType.Float:
                        _animator.SetFloat(_savedParameters[i].name, (float)_savedParameterValues[_savedParameters[i].name]);
                        break;
                    case AnimatorControllerParameterType.Int:
                        _animator.SetFloat(_savedParameters[i].name, (int)_savedParameterValues[_savedParameters[i].name]);
                        break;
                    case AnimatorControllerParameterType.Trigger:
                    default:
                        // we don't care
                        break;
                }
            }
        }
    }
}
