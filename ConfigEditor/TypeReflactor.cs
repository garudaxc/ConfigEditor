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

            //if (info_ != null)
            //{
            //    Log.Append("field {0} is {1}a value type", info_.Name, type_.IsValueType ? "" : "not ");
            //    Log.Append("field {0} is {1}a array", info_.Name, type_.IsArray ? "" : "not ");
            //}

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

            FieldInfo[] fields = type_.GetFields(BindingFlags.NonPublic |
               BindingFlags.Public |
               BindingFlags.Instance);

            for (int i = 0; i < fields.Length; i++)
            {
                children_.Add(new FieldNode(fields[i].FieldType, fields[i]));
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

        public void SerializeTo(string path)
        {
            //BinaryFormatter serializer = new BinaryFormatter();
            //DataContractJsonSerializer 

            DataContractSerializer serializer = new DataContractSerializer(type_);
            FileStream stream = new FileStream(path, FileMode.Create);
            //serializer.Serialize(stream, value_);
            serializer.WriteObject(stream, value_);

            stream.Close();
        }

        public void SerializeFrom(string path)
        {
            DataContractSerializer serializer = new DataContractSerializer(type_);
            FileStream stream = new FileStream(path, FileMode.Open);
            //serializer.Serialize(stream, value_);
            value_ = serializer.ReadObject(stream);

            stream.Close();
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
