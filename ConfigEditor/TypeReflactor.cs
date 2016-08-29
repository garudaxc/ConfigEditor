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

        static Object CreateValue(Type t, FieldInfo info)
        {
            // todo use info to init value
            // enum ?

            if (t.Equals(typeof(Int32)))
            {
                return new Int32();
            }
            else if (t.Equals(typeof(Int16)))
            {
                return new Int16();
            }
            else if (t.Equals(typeof(Byte)))
            {
                return new Byte();
            }
            else if (t.Equals(typeof(UInt32)))
            {
                return new UInt32();
            }
            else if (t.Equals(typeof(UInt16)))
            {
                return new UInt16();
            }
            else if (t.Equals(typeof(Single)))
            {
                return new Single();
            }
            else if (t.Equals(typeof(Double)))
            {
                return new Double();
            }
            else if (t.Equals(typeof(Boolean)))
            {
                return new Boolean();
            }

            // throw a exception
            throw new Exception(string.Format("unknow value type {0}", t.Name));

            return null;
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
                value_ = CreateValue(t, info);
                return;
            }

            if(type_.Equals(typeof(string)))
            {
                value_ = string.Empty;
                return;
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

            ConstructorInfo ct = type_.GetConstructor(Type.EmptyTypes);
            if (ct == null)
            {
                Log.Append("can not find none params constructor of type : {0}", type_.Name);
            }
            else
            {
                Object[] parameters = new Object[0];
                try
                {
                    value_ = ct.Invoke(parameters);
                    Log.Append("construct a {0} object", value_.ToString());

                }
                catch (System.Exception ex)
                {
                    Log.Append("error in invoke constructor of type {0} : {1}", type_.Name, ex.ToString());
                }
            }

            FieldInfo[] fields = type_.GetFields(BindingFlags.NonPublic |
               BindingFlags.Public |
               BindingFlags.Instance);

            for (int i = 0; i < fields.Length; i++)
            {
                children_.Add(new FieldNode(fields[i].FieldType, fields[i]));
            }
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
