using System.Collections.Generic;
using System.IO;
using System.Reflection;
using UnityEngine;
using UnityEditor;

public class ComponentCopier
{
    static bool debug = false;
    private class ComponentField : System.Collections.IEnumerable
    {
        public string name;
        public object value = null;
        public string type;
        public string subtree = null;
        public bool isRef;
        //public string path = null;

        private List<ComponentField> tree = new List<ComponentField>();

        private ComponentField()
        {
        }

        public ComponentField(Component c)
        {
            name = "ROS";
            type = c.GetType().ToString();
        }

        private static ComponentField LoadEntry(string name, int index)
        {
            ComponentField ret = new ComponentField();
            ret.name = PlayerPrefs.GetString(name + index + "Name");
            ret.type = PlayerPrefs.GetString(name + index + "Type");
            ret.subtree = PlayerPrefs.GetString(name + index + "Subtree", null);
            ret.isRef = PlayerPrefs.GetInt(name + index + "Ref") == 1;

            System.Type t = System.Type.GetType(ret.type);
            if (t != null && IsBasicType(t))
            {
                if (t == typeof(bool))
                {
                    ret.value = PlayerPrefs.GetInt(name + index + "Val") == 1;
                }
                else if (t == typeof(int) || t.IsEnum)
                {
                    ret.value = PlayerPrefs.GetInt(name + index + "Val");
                }
                else if (t == typeof(float))
                {
                    ret.value = PlayerPrefs.GetFloat(name + index + "Val");
                }
                else
                {
                    ret.value = PlayerPrefs.GetString(name + index + "Val", null);
                }
            }
            else if (ret.isRef)
            {
                int id = PlayerPrefs.GetInt(name + index + "Val");
                ret.value = findObject(id);
                //ret.path = PlayerPrefs.GetString(name + index + "Path", null);
            }
            else
            {
                ret.value = PlayerPrefs.GetString(name + index + "Val", "");
                if ((string)ret.value == "" && PlayerPrefs.GetString(name + index + "Val", "du") == "du")
                {
                    ret.value = PlayerPrefs.GetInt(name + index + "Val", 0);
                    if ((int)ret.value == 0 && PlayerPrefs.GetInt(name + index + "Val", 1) == 1)
                        ret.value = null;
                }
            }

            return ret;
        }

        private static Object findObject(int id)
        {
            if (AssetDatabase.Contains(id))
            {
                Object[] items = AssetDatabase.LoadAllAssetsAtPath(AssetDatabase.GetAssetPath(id));
                foreach (Object o in items)
                {
                    if (o.GetInstanceID() == id)
                        return o;
                }
            }

            Object[] sceneitems = Object.FindObjectsOfType(typeof(Object));

            foreach (Object o in sceneitems)
            {
                if (o.GetInstanceID() == id)
                    return o;
            }

            return null;
        }

        public static ComponentField LoadTree(string name)
        {
            string treeName = "ROS" + name;

            return LoadSubTree(treeName);
        }

        public ComponentField GetEntry(int i)
        {
            return tree[i];
        }

        public static ComponentField LoadSubTree(string path)
        {
            ComponentField ret = new ComponentField();
            ret.type = PlayerPrefs.GetString(path + "Type");

            ComponentField[] children = m_LoadTree(path);

            foreach (ComponentField cf in children)
            {
                ret.tree.Add(cf);
            }

            return ret;
        }

        private static ComponentField[] m_LoadTree(string name)
        {
            int cnt = PlayerPrefs.GetInt(name + "Count", 0);
            ComponentField[] ret = new ComponentField[cnt];
            for (int i = 0; i < cnt; i++)
            {
                ret[i] = LoadEntry(name, i);
            }
            return ret;
        }

        public ComponentField AddEntry(string name, object value)
        {
            ComponentField ret = new ComponentField();
            ret.name = name;
            if (value != null)
            {
                ret.type = value.GetType().ToString();
                if (IsBasicType(value))
                {
                    ret.value = value;
                }
                else if (value is Object)
                {
                    ret.value = value;
                    ret.isRef = true;
                    //ret.path = AssetDatabase.GetAssetPath(value as Object);
                }
            }

            tree.Add(ret);
            return ret;
        }

        public void SaveTree(string name)
        {
            DeleteTree(name);
            m_SaveTree(this.name + name);
        }

        public int Length { get { return tree.Count; } }

        private void m_SaveTree(string path)
        {
            int cnt = tree.Count;
            if (cnt == 0)
                return;
            PlayerPrefs.SetInt(path + "Count", cnt);
            PlayerPrefs.SetString(path + "Type", type);

            for (int i = 0; i < cnt; i++)
            {
                string subtreeName = path + i + "t" + tree[i].name;
                tree[i].SaveEntry(path + i, subtreeName);
                tree[i].m_SaveTree(subtreeName);
            }
        }

        private void SaveEntry(string path, string subtreeName)
        {
            PlayerPrefs.SetString(path + "Name", name);
            PlayerPrefs.SetString(path + "Type", type);
            if (tree.Count > 0)
                PlayerPrefs.SetString(path + "Subtree", subtreeName);
            PlayerPrefs.SetInt(path + "Ref", isRef ? 1 : 0);

            if (isRef)
            {
                PlayerPrefs.SetInt(path + "Val", (value as Object).GetInstanceID());
                //if (this.path != null && this.path != "")
                //PlayerPrefs.SetString(path + "Path", this.path);
            }
            else if (value != null)
            {
                if (value is bool)
                    PlayerPrefs.SetInt(path + "Val", (bool)value ? 1 : 0);
                else if (value is int || value.GetType().IsEnum)
                    PlayerPrefs.SetInt(path + "Val", (int)value);
                else if (value is float)
                    PlayerPrefs.SetFloat(path + "Val", (float)value);
                else
                    PlayerPrefs.SetString(path + "Val", value.ToString());
            }
        }

        [MenuItem("CONTEXT/Component/Clear", true)]
        public static bool CanDeltree(MenuCommand command)
        {
            //Component component = (Component)command.context;

            ComponentField tree = ComponentField.LoadTree("");

            //System.Type t = component.GetType();

            return tree.type != "";
        }

        [MenuItem("CONTEXT/Component/Clear")]
        public static void DeleteTree()
        {
            m_DeleteTree("ROS");
        }

        public static void DeleteTree(string name)
        {
            m_DeleteTree("ROS" + name);
        }

        private static void m_DeleteTree(string path)
        {
            ComponentField[] tree = m_LoadTree(path);
            int cnt = tree.Length;
            for (int i = 0; i < cnt; i++)
            {
                string subtreeName = path + i + "t" + tree[i].name;
                DeleteEntry(path + i, subtreeName);
                m_DeleteTree(subtreeName);
            }

            PlayerPrefs.DeleteKey(path + "Count");
            PlayerPrefs.DeleteKey(path + "Type");
        }

        private static void DeleteEntry(string path, string subtreeName)
        {
            PlayerPrefs.DeleteKey(path + "Name");
            PlayerPrefs.DeleteKey(path + "Type");
            PlayerPrefs.DeleteKey(path + "Subtree");
            PlayerPrefs.DeleteKey(path + "Ref");
            PlayerPrefs.DeleteKey(path + "Val");
        }

        public ComponentField FindEntry(string name)
        {
            foreach (ComponentField f in this)
            {
                if (f.name == name)
                    return f;
            }
            return null;
        }

        System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator()
        {
            return tree.GetEnumerator();
        }
    }

    private static bool SerializeField(object o, string field, System.Type type)
    {
        return SerializeField(o, field, type, 0);
    }

    private static bool SerializeField(object o, string field, System.Type type, int parameters)
    {
        if (parameters != 0)
            return false;
        if (type == typeof(SerializedObject) || type == typeof(SerializedProperty))
            return false;

        if (o is Object)
        {
            if (field == "name" || field == "active" || field == "tag" || field == "hideFlags" || field == "enabled")
                return false;

            if (o is Renderer)
            {
                if (field == "materials" || field == "material" || field == "sharedMaterial")
                    return false;
            }
            else if (o is Collider)
            {
                if (field == "material")
                    return false;
            }
            else if (o is MeshFilter)
            {
                if (field == "mesh")
                    return false;
            }
            else if (o is MeshCollider)
            {
                if (field == "mesh")
                    return false;
            }
            else if (o is Transform)
            {
                if (field.StartsWith("local") && field != "localScale" || field == "parent" || field == "right" || field == "up" || field == "forward")
                    return false;
            }
        }
        else if (o is Quaternion)
        {
            if (field == "eulerAngles")
                return false;
        }

        return true;
    }

    private static bool IsBasicType(object o)
    {
        System.Type t = o.GetType();
        return IsBasicType(t);
    }

    private static bool IsBasicType(System.Type t)
    {
        return (t == typeof(string)) || t.IsPrimitive || t.IsEnum;
    }

    private static void SerializeArray(object obj, int depth, ComponentField tree)
    {
        tree.name += " a";

        System.Array arr = obj as System.Array;

        System.Type elemType = arr.GetType().GetElementType();

        for (int i = 0; i < arr.Length; i++)
        {
            if (IsBasicType(elemType))
            {
                tree.AddEntry("", arr.GetValue(i));
            }
            else if (elemType.IsSubclassOf(typeof(Object)) || elemType==typeof(Object))
            {
                tree.AddEntry("", arr.GetValue(i));
            }
            else
            {
                Serialize(arr.GetValue(i), depth, tree.AddEntry("", null));
            }
        }
    }

    private static void Serialize(object obj, int depth, ComponentField tree)
    {
        if (obj == null)
            return;

        System.Type t = obj.GetType();


        FieldInfo[] fieldInfoArray = t.GetFields(BindingFlags.Public | BindingFlags.Instance | BindingFlags.SetProperty);
        foreach (FieldInfo mbrInfo in fieldInfoArray)
        {
            if (SerializeField(obj, mbrInfo.Name, mbrInfo.FieldType) && mbrInfo.GetCustomAttributes(typeof(System.NonSerializedAttribute), true).Length == 0)
            {
                object val = mbrInfo.GetValue(obj);
                if (val != null)
                {
                    ComponentField subtree = tree.AddEntry(mbrInfo.Name, val);

                    if (!IsBasicType(val))
                    {
                        if (val.GetType().IsArray)
                        {
                            SerializeArray(val, depth, subtree);
                        }
                        else if (!(val is Object) && depth < 2)
                        {
                            Serialize(val, depth + 1, subtree);
                        }
                    }
                }
            }
        }

        PropertyInfo[] propInfoArray = t.GetProperties(BindingFlags.Public | BindingFlags.Instance | BindingFlags.SetProperty);
        foreach (PropertyInfo mbrInfo in propInfoArray)
        {
            if (SerializeField(obj, mbrInfo.Name, mbrInfo.PropertyType, mbrInfo.GetIndexParameters().Length))
            {
                if (mbrInfo.CanRead && mbrInfo.GetSetMethod() != null && mbrInfo.GetCustomAttributes(typeof(System.NonSerializedAttribute), true).Length == 0)
                {
                    object val = mbrInfo.GetValue(obj, null);
                    if (val != null)
                    {
                        ComponentField subtree = tree.AddEntry(mbrInfo.Name, val);
                        if (!IsBasicType(val))
                        {
                            if (val.GetType().IsArray)
                            {
                                SerializeArray(val, depth, subtree);
                            }
                            else if (!(val is Object) && depth < 2)
                            {
                                Serialize(val, depth + 1, subtree);
                            }
                        }
                    }
                }
            }
        }

        SerializeCustom(obj, depth, tree);
    }

    private static void SerializeCustom(object obj, int depth, ComponentField tree)
    {
        if (obj is Animation)
        {
            ComponentField subtree = tree.AddEntry(" eAnimStates", null);

            Animation comp = obj as Animation;

            int i = 0;
            foreach (AnimationState s in comp)
            {
                ComponentField sTree = subtree.AddEntry((i++).ToString(), null);
                Serialize(s, depth, sTree);
                sTree.AddEntry("clip", s.clip);
            }
        }
    }


    private static object DeserializeArray(System.Type arrType, ComponentField tree)
    {
        System.Type elemType = arrType.GetElementType();

        System.Array arr = System.Array.CreateInstance(elemType, tree.Length);

        for (int i = 0; i < arr.Length; i++)
        {
            if (elemType.IsArray)
            {
                arr.SetValue(DeserializeArray(arrType, ComponentField.LoadSubTree(tree.GetEntry(i).subtree)), i);
            }
            else if (IsBasicType(elemType) || elemType.IsSubclassOf(typeof(Object)) || elemType == typeof(Object))
            {
                arr.SetValue(tree.GetEntry(i).value, i);
            }
            else if (!(elemType.IsSubclassOf(typeof(Object))) && elemType != typeof(Object))
            {
                arr.SetValue(DeserializeStruct(elemType, ComponentField.LoadSubTree(tree.GetEntry(i).subtree)), i);
            }
            else
            {
                arr.SetValue(GetInstance(elemType), i);
                Deserialize(arr.GetValue(i), ComponentField.LoadSubTree(tree.GetEntry(i).subtree));
            }
        }
        return arr;
    }

    private static object GetInstance(System.Type t)
    {
        object ret;
        if (t.IsArray)
        {
            ret = null;
        }
        else if (t.IsClass)
        {
            ConstructorInfo ctor = t.GetConstructor(System.Type.EmptyTypes);
            if (ctor == null)
            {
                if (debug) Debug.Log("No constructor for " + t);
                return null;
            }
            ret = ctor.Invoke(null);
        }
        else
        {
            ret = System.Activator.CreateInstance(t);
        }
        return ret;
    }

    private static object DeserializeStruct(System.Type t, ComponentField tree)
    {
        object ret = GetInstance(t);
        if (ret != null)
        {
            Deserialize(ret, tree);
        }
        else
        {
            if (debug) Debug.Log("No constructor for " + t);
        }
        return ret;
    }

    private static void Deserialize(object obj, ComponentField tree)
    {
        System.Type t = obj.GetType();

        if (tree.type != "" && tree.type != t.ToString())
        {
            if (debug) Debug.LogWarning("Type mismatch: " + tree.type + ", " + t.ToString());
            return;
        }

        FieldInfo[] fieldInfoArray = t.GetFields(BindingFlags.Public | BindingFlags.Instance | BindingFlags.SetProperty);
        foreach (FieldInfo mbrInfo in fieldInfoArray)
        {
            if (SerializeField(obj, mbrInfo.Name, mbrInfo.FieldType))
            {
                ComponentField f = tree.FindEntry(mbrInfo.FieldType.IsArray ? mbrInfo.Name + " a" : mbrInfo.Name);
                if (f == null)
                {
                    if (debug) Debug.Log("Entry " + mbrInfo.Name + " not found");
                    continue;
                }

                System.Type val = mbrInfo.FieldType;

                try
                {

                    if (!IsBasicType(val))
                    {
                        if (val.IsArray)
                        {
                            mbrInfo.SetValue(obj, DeserializeArray(val, ComponentField.LoadSubTree(f.subtree)));
                        }
                        else if (!(val.IsSubclassOf(typeof(Object))) && val != typeof(Object))
                        {
                            mbrInfo.SetValue(obj, DeserializeStruct(val, ComponentField.LoadSubTree(f.subtree)));
                        }
                        else
                        {
                            mbrInfo.SetValue(obj, f.value);
                        }
                    }
                    else
                    {
                        if (mbrInfo.FieldType.IsEnum)
                        {

                            f.value = System.Enum.ToObject(mbrInfo.FieldType, f.value);
                        }
                        mbrInfo.SetValue(obj, f.value);
                    }
                }
                catch (System.Exception e)
                {
                    if (debug) Debug.LogWarning("Restoring " + mbrInfo.Name + " failed\n" + e);
                }
            }
        }

        PropertyInfo[] propInfoArray = t.GetProperties(BindingFlags.Public | BindingFlags.Instance | BindingFlags.SetProperty);
        foreach (PropertyInfo mbrInfo in propInfoArray)
        {
            if (SerializeField(obj, mbrInfo.Name, mbrInfo.PropertyType, mbrInfo.GetIndexParameters().Length))
            {
                if (mbrInfo.CanRead && mbrInfo.GetSetMethod() != null)
                {

                    ComponentField f = tree.FindEntry(mbrInfo.PropertyType.IsArray ? mbrInfo.Name + " a" : mbrInfo.Name);
                    System.Type val = mbrInfo.PropertyType;
                    if (f == null)
                    {
                        if (debug) Debug.Log("Entry " + mbrInfo.Name + " not found");
                        continue;
                    }
                    try
                    {
                        if (!IsBasicType(val))
                        {
                            if (val.IsArray)
                            {
                                mbrInfo.SetValue(obj, DeserializeArray(val, ComponentField.LoadSubTree(f.subtree)), null);
                            }
                            else if (!(val.IsSubclassOf(typeof(Object))) && val != typeof(Object))
                            {
                                mbrInfo.SetValue(obj, DeserializeStruct(val, ComponentField.LoadSubTree(f.subtree)), null);
                            }
                            else
                            {
                                mbrInfo.SetValue(obj, f.value, null);
                            }
                        }
                        else
                        {
                            if (mbrInfo.PropertyType.IsEnum)
                            {
                                f.value = System.Enum.ToObject(mbrInfo.PropertyType, f.value);
                            }
                            mbrInfo.SetValue(obj, f.value, null);
                        }
                    }
                    catch (System.Exception e)
                    {
                        if (debug) Debug.LogWarning("Restoring " + mbrInfo.Name + "(" + mbrInfo.PropertyType + ") failed. " + (obj != null) + " " + (f != null) + (f != null ? ", " + f.value : "") + "\n" + e.ToString());
                    }
                }
            }
        }

        DeserializeCustom(obj, tree);
    }

    private static void DeserializeCustom(object obj, ComponentField tree)
    {
        if (obj is Animation)
        {
            ComponentField subtree = ComponentField.LoadSubTree(tree.FindEntry(" eAnimStates").subtree);
            Animation comp = obj as Animation;
            List<AnimationState> slist = new List<AnimationState>();

            foreach (AnimationState s in comp)
            {
                slist.Add(s);
            }
            foreach (AnimationState s in slist)
            {
                comp.RemoveClip(s.name);
            }
            slist.Clear();

            for (int i = 0; i < subtree.Length; i++)
            {
                ComponentField sTree = ComponentField.LoadSubTree(subtree.GetEntry(i).subtree);
                comp.AddClip(sTree.FindEntry("clip").value as AnimationClip, sTree.FindEntry("name").value as string);
                AnimationState cs = comp[sTree.FindEntry("name").value as string];
                cs.blendMode = (AnimationBlendMode)sTree.FindEntry("blendMode").value;
                cs.enabled = (bool)sTree.FindEntry("enabled").value;
                cs.layer = (int)sTree.FindEntry("layer").value;
                cs.speed = (float)sTree.FindEntry("speed").value;
                cs.weight = (float)sTree.FindEntry("weight").value;
                cs.wrapMode = (WrapMode)sTree.FindEntry("wrapMode").value;
            }
        }
    }

    [MenuItem("CONTEXT/Component/Copy")]
    static void Copy(MenuCommand command)
    {
        Component component = (Component)command.context;

        ComponentField tree = new ComponentField(component);
        Serialize(component, 0, tree);
        tree.SaveTree("");
    }

    [MenuItem("CONTEXT/Component/Paste", true)]
    public static bool CanPaste(MenuCommand command)
    {
        Component component = (Component)command.context;

        ComponentField tree = ComponentField.LoadTree("");

        System.Type t = component.GetType();

        return tree.type != "" && tree.type == t.ToString();
    }

    [MenuItem("CONTEXT/Component/Paste")]
    static void Paste(MenuCommand command)
    {
        Component component = (Component)command.context;

        Undo.RegisterUndo(component, "Paste properties to compinent");
        ComponentField tree = ComponentField.LoadTree("");
        Deserialize(component, tree);
    }

    [MenuItem("CONTEXT/Component/Paste new component", true)]
    public static bool CanPaste2(MenuCommand command)
    {
        //Component component = (Component)command.context;

        ComponentField tree = ComponentField.LoadTree("");

        //System.Type t = component.GetType();

        return tree.type != "";
    }

    [MenuItem("CONTEXT/Component/Paste new component")]
    static void Paste2(MenuCommand command)
    {

        //window.clipboardData.setData("text","This has been copied to your clipboard.");
        GameObject go = (command.context as Component).gameObject;


        ComponentField tree = ComponentField.LoadTree("");
        Undo.RegisterUndo(go, "Paste properties to new component");
        Component component = UnityEngineInternal.APIUpdaterRuntimeServices.AddComponent(go, "Assets\\Scripts\\Plugins\\Editor\\ComponentCopier.cs (712,31)", tree.type);

        if (component != null)
        {

            Deserialize(component, tree);
        }
    }
}
