using Oculus.Platform;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
namespace Juul
{
    public class Button
    {
        public string Name = "Placeholder";
        public string Description = "Placeholder button.";
        private bool _enabled = false;
        public bool Enabled
        {
            get { return _enabled; }
            set
            {
                if (_enabled == value) return;
                _enabled = value;
                if (_enabled)
                {
                    if (!Core.ActiveButtons.Contains(this))
                        Core.ActiveButtons.Add(this);
                }
                else
                {
                    Core.ActiveButtons.Remove(this);
                }
            }
        }
        public bool Toggle = true;
        public Action OnEnable = () => { };
        public Action OnDisable = () => { };
        public Action OnceEnable = () => { };
        public Action OnceDisable = () => { };
        public Action OnUpdate = () => { };
        public bool Label = false;
        public bool Incremental = false;
        public Action Up = () => { };
        public Action Down = () => { };
        public bool HasKeybinds = false;
        public Category KeybindCategory = null;
        public void SetText(string NewText)
        {
            this.Name = NewText;
            Core.RebuildMenu();
        }
        public void SetEnabled(bool enabled)
        {
            this.Enabled = enabled;
            Core.RebuildMenu();
        }
    }
    public class Category
    {
        public string Name = "Placeholder";
        public Category ParentCategory = null;
        public List<Button> Buttons = new List<Button>();
        public List<Category> Subcategories = new List<Category>();
        public Category Add(Category sub)
        {
            sub.ParentCategory = this;
            this.Subcategories.Add(sub);
            return this;
        }
        public Button GetButton(string name)
        {
            return this.Buttons.FirstOrDefault(b => b.Name == name);
        }
    }
    public class SubCategory : Category { }

    public class ButtonCollider : MonoBehaviour
    {
        public Action onClick;
        private void OnTriggerEnter(Collider other)
        {
            if ((other.gameObject == Core.Pointer || other.gameObject == Core.Pointer2) && Time.time > Core.ButtonCooldown)
            {
                Core.ButtonCooldown = Time.time + 0.2345f;
                onClick?.Invoke();
                Audios.Play("Select");
                Core.RebuildMenu();
            }
        }
    }

    public class IncrementalButtonCollider : MonoBehaviour
    {
        public Action onClick;
        private void OnTriggerEnter(Collider other)
        {
            if ((other.gameObject == Core.Pointer || other.gameObject == Core.Pointer2) && Time.time > Core.IncrementCooldown)
            {
                Core.IncrementCooldown = Time.time + 0.15f;
                onClick?.Invoke();
                Audios.Play("Select");
                Core.RebuildMenu();
            }
        }
    }

    public partial class rebuld : MonoBehaviour
    {
        public static void RebuildMenu()
        {
            if (Core.Menu != null)
            {
                GameObject.Destroy(Core.Menu);
                Core.Menu = null;
            }
            Core.CreateFrame();
        }
    }
}

