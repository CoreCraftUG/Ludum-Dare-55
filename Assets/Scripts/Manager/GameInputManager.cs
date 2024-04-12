using System;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.Users;

namespace JamCraft.GMTK2023.Code
{
    public class GameInputManager : MonoBehaviour
    {
        public static GameInputManager Instance;

        private GameInput _gameInput;
        private ControlScheme _currentControlScheme;
        private InputUser _currentUser;
        private string _currentCancelKey;

        #region EventHandlers

        public event EventHandler OnTurnTableRightAction;
        public event EventHandler OnTurnTableLeftAction;
        public event EventHandler OnPlaceCardAction;
        public event EventHandler OnPauseAction;

        #endregion

        public event EventHandler<InputBinding> OnDuplicateKeybindingFound;
        public event EventHandler<ControlScheme> OnInputDeviceChanged;

        /// <summary>
        /// All actions in the game.
        /// </summary>
        public enum Actions
        {
            TurnTableRight,
            TurnTableLeft,
            PlaceCard
        }

        /// <summary>
        /// All possible control schemes, which are currently supported in the game.
        /// </summary>
        public enum ControlScheme
        {
            Keyboard,
            Gamepad
        }

        private void Awake()
        {
            if (Instance != null)
            {
                Debug.LogError($"There is more than one {this} instance in the scene!");
            }

            Instance = this;

            DontDestroyOnLoad(this.gameObject);

            _gameInput = new GameInput();
            _gameInput.Player.Enable();
            
            _currentUser = InputUser.PerformPairingWithDevice(Keyboard.current); // Set Keyboard as default device.
            _currentUser.AssociateActionsWithUser(_gameInput);
            _currentUser.ActivateControlScheme(_gameInput.KeyboardScheme);
            _currentControlScheme = ControlScheme.Keyboard;

            InputUser.listenForUnpairedDeviceActivity++;
            InputUser.onUnpairedDeviceUsed += OnUnpairedDeviceUsed;

            // Check if user has custom bindings and load from the save file if available.

            if (ES3.KeyExists(GameSettingsFile.USERSETTINGS_INPUT_BINDINGS, GameSettingsFile.Instance.UserSettingsFilePath, GameSettingsFile.Instance.ES3Settings))
            {
                _gameInput.LoadBindingOverridesFromJson(ES3.Load<string>(GameSettingsFile.USERSETTINGS_INPUT_BINDINGS, GameSettingsFile.Instance.UserSettingsFilePath, GameSettingsFile.Instance.ES3Settings));
            }

            RegisterInputActions();
        }

        private void Start()
        {
            OnInputDeviceChanged += SetKeybindBindingCancelKey;
        }

        /// <summary>
        /// Set the key to cancel a keybind binding process. Default for Keyboard is Escape and for Gamepad is east button.
        /// </summary>
        /// <param name="controlScheme">Name of the control scheme to differentiate what key to use.</param>
        private void SetKeybindBindingCancelKey(object sender, ControlScheme controlScheme)
        {
            switch (controlScheme)
            {
                default:
                case ControlScheme.Keyboard:
                    _currentCancelKey = "<Keyboard>/escape";
                    break;
                case ControlScheme.Gamepad:
                    _currentCancelKey = "<Gamepad>/eastButton";
                    break;
            }
        }

        private void OnUnpairedDeviceUsed(InputControl inputControl, UnityEngine.InputSystem.LowLevel.InputEventPtr inputEventPtr)
        {
            ControlScheme cacheControlScheme = _currentControlScheme; // Cache current control scheme.

            InputDevice inputDevice = inputControl.device; // Get the unpaired device.

            if (inputDevice is Gamepad)
            {
                _currentUser.UnpairDevices();

                InputUser.PerformPairingWithDevice(Gamepad.current, user: _currentUser); // Pair gamepad.

                _currentUser.ActivateControlScheme(ControlScheme.Gamepad.ToString()); // Activate the control scheme of the gamepad.
                
                _currentControlScheme = ControlScheme.Gamepad; // Set gamepad as active control scheme.
            }

            if (inputDevice is Keyboard || inputDevice is Mouse)
            {
                _currentUser.UnpairDevices();

                // Pair Keyboard and Mouse. 
                InputUser.PerformPairingWithDevice(Keyboard.current, user: _currentUser);
                InputUser.PerformPairingWithDevice(Mouse.current, user: _currentUser);

                _currentUser.ActivateControlScheme(ControlScheme.Keyboard.ToString()); // Activate the control scheme of the keyboard.

                _currentControlScheme = ControlScheme.Keyboard; // Set keyboard as active control scheme.
            }

            if (cacheControlScheme != _currentControlScheme) // Check if the control scheme has changed and send an event to notify listeners.
            {
                OnInputDeviceChanged?.Invoke(this, _currentControlScheme);
                //Debug.Log($"Device changed to {_currentControlScheme}.");
            }
        }

        /// <summary>
        /// Reset all bindings for all actions in all input action maps in the input action asset for the current control scheme and save the new bindings to the save file.
        /// </summary>
        public void GameOptionsUI_OnResetToDefault(object sender, EventArgs e)
        {
            foreach (InputActionMap map in _gameInput.asset.actionMaps)
            {
                foreach (InputAction inputAction in map.actions)
                {
                    inputAction.RemoveBindingOverride(InputBinding.MaskByGroup(_currentControlScheme.ToString()));
                    ES3.Save(GameSettingsFile.USERSETTINGS_INPUT_BINDINGS, _gameInput.SaveBindingOverridesAsJson(), GameSettingsFile.Instance.UserSettingsFilePath, GameSettingsFile.Instance.ES3Settings);
                }
            }
        }

        private void PlaceCard_performed(InputAction.CallbackContext obj)
        {
            OnPlaceCardAction?.Invoke(this, EventArgs.Empty);
        }

        private void TurnTableCounterClockwise_performed(InputAction.CallbackContext obj)
        {
            OnTurnTableLeftAction?.Invoke(this, EventArgs.Empty);
        }

        private void TurnTableClockwise_performed(InputAction.CallbackContext obj)
        {
            OnTurnTableRightAction?.Invoke(this, EventArgs.Empty);
        }

        private void Pause_performed(InputAction.CallbackContext obj)
        {
            OnPauseAction?.Invoke(this, EventArgs.Empty);
        }


        /// <summary>
        /// Subscribe to the performed state of all input actions.
        /// </summary>
        private void RegisterInputActions()
        {
            _gameInput.Player.TurnTableRight.performed += TurnTableClockwise_performed;
            _gameInput.Player.TurnTableLeft.performed += TurnTableCounterClockwise_performed;
            _gameInput.Player.PlaceCard.performed += PlaceCard_performed;
            _gameInput.Player.Pause.performed += Pause_performed;
            //InputSystem.onAnyButtonPress.CallOnce(control => Debug.Log("Test"));
        }

        /// <summary>
        /// Return the binding of an action as string.
        /// </summary>
        /// <param name="actions">Respective action to get the binding from.</param>
        /// <param name="bindingIndex">Respective index of the binding to return as string.</param>
        /// <returns></returns>
        public string GetBindingText(Actions actions, int bindingIndex)
        {
            switch (actions)
            {
                default:
                case Actions.TurnTableRight:
                    return _gameInput.Player.TurnTableRight.bindings[bindingIndex].ToDisplayString();
                case Actions.TurnTableLeft:
                    return _gameInput.Player.TurnTableLeft.bindings[bindingIndex].ToDisplayString();
                case Actions.PlaceCard:
                    return _gameInput.Player.PlaceCard.bindings[bindingIndex].ToDisplayString();
            }
        }

        /// <summary>
        /// Rebind an action.
        /// </summary>
        /// <param name="actions">Actions in the game. Used to determine what action is about to be rebound.</param>
        /// <param name="onActionRebound">Custom event that gets invoked after an action is rebound.</param>
        /// <param name="bindingIndex">Respective index of the binding that gets rebound.</param>
        public void RebindBinding(Actions actions, Action onActionRebound, int bindingIndex/*, bool allCompositeParts = false*/)
        {
            //_gameInput.Player.Disable();

            InputAction inputAction;

            switch (actions)
            {
                default:
                case Actions.TurnTableRight:
                    inputAction = _gameInput.Player.TurnTableRight;
                    break;
                case Actions.TurnTableLeft:
                    inputAction = _gameInput.Player.TurnTableLeft;
                    break;
                case Actions.PlaceCard:
                    inputAction = _gameInput.Player.PlaceCard;
                    break;
            }

            inputAction.Disable(); // Needs to be disabled, otherwise no rebinding can be performed.

            inputAction.PerformInteractiveRebinding(bindingIndex)
                .WithCancelingThrough(_currentCancelKey) // Set the key to cancel the process.
                .WithControlsExcluding("<Mouse>") // Exclude Mouse as an input device to be bound to an action.
                .WithControlsExcluding("<Keyboard>/anyKey") // Exclude anyKey of the Keyboard to be bound to an action.
                .OnCancel(operation =>
                {
                    ResetBinding(inputAction, bindingIndex); // Reset to the old binding.
                    inputAction.Enable();
                    onActionRebound?.Invoke();
                    ES3.Save(GameSettingsFile.USERSETTINGS_INPUT_BINDINGS, _gameInput.SaveBindingOverridesAsJson(), GameSettingsFile.Instance.UserSettingsFilePath, GameSettingsFile.Instance.ES3Settings);
                    operation.Dispose();
                })
                .OnComplete(operation =>
                {
                    if (CheckForDuplicateBindings(inputAction, bindingIndex/*, allCompositeParts*/))
                    {
                        inputAction.RemoveBindingOverride(bindingIndex);

                        operation.Dispose();

                        RebindBinding(actions, () =>
                        {
                            GameOptionsUI.Instance.HideRebindPanel();
                            GameOptionsUI.Instance.UpdateVisual();
                        }, bindingIndex);

                        return;
                    }

                    inputAction.Enable();
                    onActionRebound?.Invoke();
                    ES3.Save(GameSettingsFile.USERSETTINGS_INPUT_BINDINGS, _gameInput.SaveBindingOverridesAsJson(), GameSettingsFile.Instance.UserSettingsFilePath, GameSettingsFile.Instance.ES3Settings);
                    operation.Dispose();
                })
                .Start();
        }

        /// <summary>
        /// Reset binding to the previous binding.
        /// </summary>
        /// <param name="inputAction">Input Action to reset the binding of.</param>
        /// <param name="bindingIndex">Respective index of the binding, which is to be reset.</param>
        private void ResetBinding(InputAction inputAction, int bindingIndex)
        {
            InputBinding newBinding = inputAction.bindings[bindingIndex]; // Get the binding.
            string oldOverridePath = newBinding.overridePath; // Cache the old binding path.

            inputAction.RemoveBindingOverride(bindingIndex); // Remove the binding.

            foreach (InputAction otherInputAction in inputAction.actionMap.actions) // Find the respective binding.
            {
                if (otherInputAction == inputAction)
                {
                    continue;
                }

                for (int i = 0; i < otherInputAction.bindings.Count; i++)
                {
                    InputBinding binding = otherInputAction.bindings[i];

                    if (binding.overridePath == newBinding.path) // Check if we found the correct binding and apply the old binding path.
                    {
                        otherInputAction.ApplyBindingOverride(i, oldOverridePath);
                    }
                }
            }
        }

        /// <summary>
        /// Check for duplicate bindings in the Input Action.
        /// </summary>
        /// <param name="inputAction">Input Action to check for duplicate bindings.</param>
        /// <param name="bindingIndex">Index of the respective binding to check for duplicate bindings.</param>
        /// <returns></returns>
        private bool CheckForDuplicateBindings(InputAction inputAction, int bindingIndex/*, bool allCompositeParts = false*/)
        {
            InputBinding newBinding = inputAction.bindings[bindingIndex];

            foreach (InputBinding binding in inputAction.actionMap.bindings) // Find the respective binding.
            {
                if (binding.action == newBinding.action)
                {
                    continue;
                }

                if (binding.effectivePath == newBinding.effectivePath) // Check if the bindings have the same key.
                {
                    OnDuplicateKeybindingFound?.Invoke(this, binding);
                    // TODO: Highlight keybinding?
                    return true;
                }
            }

            //if (allCompositeParts)
            //{
            //    for (int i = 1; i < bindingIndex; i++)
            //    {
            //        if (inputAction.bindings[i].effectivePath == newBinding.overridePath)
            //        {
            //            Debug.Log($"Duplicate actions found: {newBinding.effectivePath}!");
            //            return true;
            //        }
            //    }
            //}

            return false;
        }

        private void OnDestroy()
        {
            if (GameOptionsUI.Instance != null)
            {
                GameOptionsUI.Instance.OnResetToDefault -= GameOptionsUI_OnResetToDefault;
            }
        }

        private void OnApplicationQuit()
        {
            if (GameOptionsUI.Instance != null)
            {
                GameOptionsUI.Instance.OnResetToDefault -= GameOptionsUI_OnResetToDefault;
            }
        }
    }
}