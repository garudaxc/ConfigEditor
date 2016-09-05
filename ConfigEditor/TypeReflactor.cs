using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Runtime.Serialization;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;

namespace ConfigEditor
{


    class FieldNode
    {
        FieldInfo info_;
        Type type_;
        Object value_;

        List<FieldNode> children_ = new List<FieldNode>();

        public string Name
        {
            get
            {
                if(info_ == null)
                {
                    return type_.Name;
                }

                return info_.Name;
            }
        }

        public List<FieldNode> Children
        {
            get
            {
                return children_;
            }
        }

        public Object Value
        {
            get
            {
                return value_;
            }

            set
            {
                value_ = value;
            }
        }

        public Type Type
        {
            get
            {
                return type_;
            }
        }

        public void CollectFieldValue(Object parent)
        {
            if (info_ == null)
            {
                throw new Exception("invalid method call fieldInfo is null");
            }

            info_.SetValue(parent, value_);
        }

        public void SetFieldValue(Object parent)
        {
            if (info_ == null)
            {
                throw new Exception("invalid method call fieldInfo is null");
            }

            value_ = info_.GetValue(parent);
        }
        
        public FieldNode(Type t, FieldInfo info = null)
        {
            type_ = t;
            info_ = info;
            
            if(type_.IsValueType)
            {
                value_ = Activator.CreateInstance(type_);
            }

            if (type_ == typeof(string))
            {
                value_ = string.Empty;
            }
        }

        public void ConstructValue()
        {
            if (type_ == null)
            {
                throw new Exception("invalid member invoke, field value_ is null");
            }

            if(value_ != null)
            {
                return;
            }

            if(type_.IsArray)
            {
                value_ = new ArrayList();
                return;
            }

            value_ = Activator.CreateInstance(type_);

            if (!type_.IsValueType && type_ != typeof(string))
            {
                FieldInfo[] fields = type_.GetFields(BindingFlags.NonPublic |
                   BindingFlags.Public |
                   BindingFlags.Instance);

                for (int i = 0; i < fields.Length; i++)
                {
                    children_.Add(new FieldNode(fields[i].FieldType, fields[i]));
                }
            }

            // init value
            DispatchInstanceValue();
        }

        public void DestroyValue()
        {
            value_ = null;
            children_.Clear();
        }

        public FieldNode CreateArrayElement()
        {
            if(!type_.IsArray)
            {
                throw new Exception("invalid invoke, not a array");
            }

            FieldNode node = new FieldNode(type_.GetElementType());
            node.ConstructValue();
            children_.Add(node);
            return node;
        }

        public void CollectInstanceValue()
        {
            children_.ForEach(node => node.CollectInstanceValue());

            if (value_ == null)
            {
                return;
            }

            if(type_.IsArray)
            {
                Array array = Array.CreateInstance(type_.GetElementType(), children_.Count);
                for(int i = 0; i < children_.Count; i++)
                {
                    array.SetValue(children_[i].Value, i);
                }
                value_ = array;
            }
            else
            {
                children_.ForEach(node => node.CollectFieldValue(value_));
            }
        }

        public void DispatchInstanceValue()
        {
            if (value_ == null)
            {
                return;
            }

            if (type_.IsArray)
            {
                children_.Clear();
                Array array = value_ as Array;
                for(int i = 0; i < array.Length; i++)
                {
                    FieldNode child = CreateArrayElement();
                    child.Value = array.GetValue(i);
                }
            }
            else
            {
                children_.ForEach(node => node.SetFieldValue(value_));
            }

            children_.ForEach(node => node.DispatchInstanceValue());
        }

        public void DispatchValue()
        {
            if (type_ == null || value_ == null)
            {
                return;
            }
            
            if (type_.IsArray)
            {
                children_.Clear();
                Array array = value_ as Array;
                for (int i = 0; i < array.Length; i++)
                {
                    FieldNode child = CreateArrayElement();
                    child.Value = array.GetValue(i);
                }
            }
            else if (!type_.IsValueType && type_ != typeof(string))
            {
                FieldInfo[] fields = type_.GetFields(BindingFlags.NonPublic |
                   BindingFlags.Public |
                   BindingFlags.Instance);

                for (int i = 0; i < fields.Length; i++)
                {
                    FieldNode node = new FieldNode(fields[i].FieldType, fields[i]);
                    node.SetFieldValue(value_);
                    children_.Add(node);
                }
            }

            children_.ForEach(node => node.DispatchValue());
        }

        public void SerializeTo(string path)
        {
            //BinaryFormatter serializer = new BinaryFormatter();
            //DataContractJsonSerializer 
            FileStream stream = new FileStream(path, FileMode.Create);

            string name = type_.FullName;

            byte[] data = System.Text.Encoding.UTF8.GetBytes(name);
            byte[] head = BitConverter.GetBytes(data.Length);
            stream.Write(head, 0, head.Length);
            stream.Write(data, 0, data.Length);

            DataContractSerializer serializer = new DataContractSerializer(type_);
            
            //serializer.Serialize(stream, value_);


            serializer.WriteObject(stream, value_);

            stream.Close();
        }

        public static FieldNode SerializeFrom(string path)
        {
            FileStream stream = new FileStream(path, FileMode.Open);
            //serializer.Serialize(stream, value_);

            byte[] buffer = new byte[256];
            stream.Read(buffer, 0, 4);
            int len = BitConverter.ToInt32(buffer, 0);
            stream.Read(buffer, 0, len);

            string typeName = System.Text.Encoding.UTF8.GetString(buffer, 0, len);

            Log.Append("read type {0}", typeName);
            Type type = null;
            foreach (LoadedAssembly ass in MainForm.Assemblys)
            {
                type = ass.assembly.GetType(typeName);
                if (type != null)
                {
                    break;
                }
            }

            if (type == null)
            {
                throw new Exception(string.Format("can not get type {0}", typeName));
            }

            DataContractSerializer serializer = new DataContractSerializer(type);
            Object value = serializer.ReadObject(stream);

            stream.Close();

            FieldNode node = new FieldNode(type);
            node.Value = value;
            return node;
        }
    }


    class TypeReflactor
    {
        Type type_;
        FieldNode root;

        public FieldNode Root
        {
            get
            {
                return root;
            }
        }

        public TypeReflactor(Type t)
        {
            type_ = t;

            root = new FieldNode(type_);
            root.ConstructValue();
            return;                        
        }
    }
}
