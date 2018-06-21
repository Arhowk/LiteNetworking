using System;
using System.CodeDom;
using System.CodeDom.Compiler;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using UnityEditor;
using UnityEngine;

namespace LiteNetworking
{
    public class PktDefGenerator
    {
        
        private static string pktClassPrefix = "pkt_";
        private static string pktClassSuffix = "_autogen";

        private static CodeCompileUnit ccu;
        private static CodeNamespace nmspc;
        private static int currentPacketId = 0;

        private static Dictionary<Type, List<KeyValuePair<Type, Type>>> serializers;


        public static void Generate()
        {
            // Star the generator
            StartGenerator();

            // Generate the data serializers
            GenerateSerializers();


            // Make all of the packet mirror classes
            var subclasses = AppDomain.CurrentDomain.GetAssemblies()
                .SelectMany(assembly => assembly.GetTypes())
                .Where(type => type.IsSubclassOf(typeof(LitePacket)));

            foreach(Type t in subclasses)
            {
                GenerateAndAppendPktDef(t);
            }
            // Write the const header
            WriteConstHeader(subclasses);

            // Write the packet sender
            WritePacketSender(subclasses);

            // Write the packet reader
            WritePacketRetriever(subclasses);

            FinalizeGenerator();
        }

        private static string GetSerializerForType(FieldInfo field)
        {
            // This is a list of < the serializer to use, the attribute required to use (or null to always use it) >
            Debug.Log("Lookup serializer for " + field.FieldType);
            Type fieldType = field.FieldType ;
            if(field.FieldType.IsArray)
            {
                fieldType = field.FieldType.GetElementType();
            }
            List<KeyValuePair<Type, Type>> types = serializers[fieldType];
            
            // Check every attribute on the field and see if one of the serializer's required attribute matches
            foreach(Attribute ab in field.GetCustomAttributes())
            {
                Debug.LogWarning("HAS CUSTOM ATTR " + ab.GetType().Name);
                for (int i = types.Count - 1; i >= 0; i--)
                {
                    if(types[i].Value == null || types[i].Value == ab.GetType())
                    {
                        // This field has the required attribute {value}, so we are going to use the serializer {key}
                        return "ser_" + types[i].Key.Name;
                    }
                }
            }
            return "ser_" + types[0].Key.Name;
        }

        public static void GenerateSerializers()
        {
            // {data to serialize, class to serialize }
            serializers = DataSerialization.GetAllSerializers();
            Debug.Log("NumSerializers : " + serializers.Keys.Count);

            CodeTypeDeclaration serializerClass = new CodeTypeDeclaration("Data_serializers_const") { IsClass = true, TypeAttributes = TypeAttributes.Public };

            foreach (KeyValuePair<Type, List<KeyValuePair<Type,Type>>> maps in serializers)
            {
                foreach(KeyValuePair<Type,Type> serializer in maps.Value)
                {
                    Debug.Log(serializer.Key + " : " + serializer.Value);
                    CodeMemberField var = new CodeMemberField(serializer.Key.Name, "ser_" + serializer.Key.Name);
                    var.Attributes = MemberAttributes.Static | MemberAttributes.Public;
                    var.InitExpression = new CodeObjectCreateExpression(serializer.Key.Name);

                    serializerClass.Members.Add(var);
                }
            }

            nmspc.Types.Add(serializerClass);
        }

        public static void WriteConstHeader(IEnumerable<Type> types)
        {
            CodeTypeDeclaration packetSenderClass = new CodeTypeDeclaration("ConstRefs") { IsClass = true, TypeAttributes = TypeAttributes.Public };

            foreach(Type t in types)
            {
                CodeMemberField var = new CodeMemberField(t.Name, "pkt_" + t.Name);
                var.Attributes = MemberAttributes.Static | MemberAttributes.Public;
                var.InitExpression = new CodeObjectCreateExpression(t.Name);

                CodeMemberField var2 = new CodeMemberField(pktClassPrefix + t.Name + pktClassSuffix, "mirror_" + t.Name);
                var2.Attributes = MemberAttributes.Static | MemberAttributes.Public;
                var2.InitExpression = new CodeObjectCreateExpression(pktClassPrefix + t.Name + pktClassSuffix);

                packetSenderClass.Members.Add(var);
                packetSenderClass.Members.Add(var2);
            }

            nmspc.Types.Add(packetSenderClass);

        }

        public static void WritePacketSender(IEnumerable<Type> types)
        {
            CodeTypeDeclaration packetSenderClass = new CodeTypeDeclaration("PacketSender") { IsClass = true, TypeAttributes = TypeAttributes.Public };


            int currentPacketId = 0;
            foreach (Type t in types)
            {
                // Sending the prepacked packet
                CodeMemberMethod m = new CodeMemberMethod();
                m.Name = "Send" + t.Name;
                m.Attributes = MemberAttributes.Static | MemberAttributes.Public;

                m.Parameters.Add(new CodeParameterDeclarationExpression(t.Name, "pkt"));
                CodeParameterDeclarationExpression senderId = new CodeParameterDeclarationExpression();
                senderId.Type = new CodeTypeReference(typeof(System.Int32));
                senderId.Name = "connectionId";
                senderId.CustomAttributes.Add(new CodeAttributeDeclaration("Optional"));
                senderId.CustomAttributes.Add(new CodeAttributeDeclaration("DefaultParameterValue", new CodeAttributeArgument(new CodePrimitiveExpression(-1))));
                m.Parameters.Add(senderId);

                // Setup the memory stream
                m.Statements.Add(
                    new CodeVariableDeclarationStatement(
                        "MemoryStream",
                        "m",
                        new CodeObjectCreateExpression("MemoryStream", new CodeExpression[] { })
                    )
                );

                // Add the packet id
                m.Statements.Add(
                    new CodeMethodInvokeExpression(
                        new CodeVariableReferenceExpression("m"),
                        "WriteByte",
                        new CodePrimitiveExpression(currentPacketId)
                    )
                );

                // Record who is sending the player
                // Networking.localPacketPlayer = LobbyConnector.ConvertConnectionToPlayer(connectionId)
                if(Attribute.GetCustomAttribute(t, typeof(LiteNetworking.RecordTargetPlayer)) != null || true) // TODO: fix this attribute
                {
                    m.Statements.Add(
                        new CodeAssignStatement(
                            new CodeFieldReferenceExpression(
                               new CodeTypeReferenceExpression(typeof(Networking)),
                               "localPacketPlayer"
                            ),

                            new CodeMethodInvokeExpression
                            (
                                new CodeTypeReferenceExpression(typeof(LobbyConnector)),
                                "ConvertConnectionToPlayer",
                                new CodeVariableReferenceExpression("connectionId")
                            )
                        )   
                    );
                }

                // Add the serialized data
                m.Statements.Add(
                    new CodeMethodInvokeExpression(
                        new CodeVariableReferenceExpression("ConstRefs.mirror_" + t.Name),
                        "_Serialize",
                        new CodeExpression[]
                        {
                            new CodeVariableReferenceExpression("pkt"),
                            new CodeVariableReferenceExpression("m")
                        }
                    )
                );

                //Transmit the packet
                m.Statements.Add(
                    new CodeMethodInvokeExpression(
                        new CodeVariableReferenceExpression("Networking"),
                        "TransmitPacket",
                        new CodeExpression[]
                        {
                            new CodeVariableReferenceExpression("m"),
                            new CodeVariableReferenceExpression("connectionId")
                        }
                    )
                );
                packetSenderClass.Members.Add(m);

                // Sending the params

                currentPacketId++;
            }

            nmspc.Types.Add(packetSenderClass);
        }

        public static void WritePacketRetriever(IEnumerable<Type> types)
        {
            CodeTypeDeclaration packetReader = new CodeTypeDeclaration("PacketReader") { IsClass = true, TypeAttributes = TypeAttributes.Public };

            // Write the list
            CodeTypeReference listType = new CodeTypeReference("List", new[] { new CodeTypeReference(typeof(M_LitePacketInternalMirror))});

            CodeMemberField list = new CodeMemberField(listType, "mirrors");
            list.InitExpression = new CodeObjectCreateExpression(listType);
            list.Attributes = MemberAttributes.Static;

            packetReader.Members.Add(list);

            // Static initializer
            CodeTypeConstructor initializer = new CodeTypeConstructor();
            initializer.Attributes = MemberAttributes.Final | MemberAttributes.Private | MemberAttributes.Static;

            foreach(Type t in types)
            {
                initializer.Statements.Add(
                    new CodeMethodInvokeExpression(
                        new CodeVariableReferenceExpression("mirrors"),
                        "Add",
                        new CodeExpression[]
                        {
                            new CodeFieldReferenceExpression(new CodeVariableReferenceExpression("ConstRefs"), "mirror_" + t.Name)
                        }
                    )
               );
            }
            packetReader.Members.Add(initializer);

            // The ReadPacket method

            CodeMemberMethod readPacket = new CodeMemberMethod();
            readPacket.Name = "ReadPacket";
            readPacket.Attributes = MemberAttributes.Public | MemberAttributes.Static;
            readPacket.Parameters.Add(new CodeParameterDeclarationExpression("MemoryStream", "m"));

            readPacket.Statements.Add(
                new CodeVariableDeclarationStatement(
                    "System.Int32",
                    "id",
                    new CodeMethodInvokeExpression(
                         new CodeVariableReferenceExpression("m"),
                         "ReadByte"
                    )
                )
           );

            readPacket.Statements.Add(
                new CodeMethodInvokeExpression(
                    new CodeArrayIndexerExpression(
                        new CodeVariableReferenceExpression("mirrors"),
                        new CodeVariableReferenceExpression("id")
                    ),
                    "Fire",
                    new CodeVariableReferenceExpression("m")
                )
            );

            packetReader.Members.Add(readPacket);

            nmspc.Types.Add(packetReader);
        }

        public static void StartGenerator()
        {
            ccu = new CodeCompileUnit();
            nmspc = new CodeNamespace("LiteNetworkingGenerated");

            nmspc.Imports.Add(new CodeNamespaceImport("System"));
            nmspc.Imports.Add(new CodeNamespaceImport("System.Collections.Generic"));
            nmspc.Imports.Add(new CodeNamespaceImport("System.IO"));
            nmspc.Imports.Add(new CodeNamespaceImport("UnityEngine"));
            nmspc.Imports.Add(new CodeNamespaceImport("LiteNetworking"));
            nmspc.Imports.Add(new CodeNamespaceImport("System.Runtime.InteropServices"));
            ccu.Namespaces.Add(nmspc);
        }

        public static void GenerateAndAppendPktDef(Type t)
        {
            // Generate the program class
            CodeTypeDeclaration programClass = new CodeTypeDeclaration(pktClassPrefix + t.Name + pktClassSuffix)
            { IsClass = true, TypeAttributes = TypeAttributes.Public | TypeAttributes.Sealed };
            programClass.BaseTypes.Add(new CodeTypeReference(typeof(M_LitePacketInternalMirror)));

            // Add the boiler plate properties to the class

            // Generate it's packetid

            // Write the serialize method
            CodeMemberMethod m = new CodeMemberMethod();
            m.Name = "_Serialize";
            m.Attributes = MemberAttributes.Public | MemberAttributes.Final; 
   
            m.Parameters.Add(new CodeParameterDeclarationExpression(t.Name, "pkt"));
            m.Parameters.Add(new CodeParameterDeclarationExpression("MemoryStream", "m"));

            var props = t.GetFields();
            Debug.Log("Type name is " + t.Name);
            // .Where(propertyInfo => propertyInfo.GetCustomAttributes(false).GetType() != typeof(LiteLocalOnly));
            foreach (FieldInfo p in props)
            {
                if (p.FieldType.IsArray)
                {
                    Type subType = p.FieldType.GetElementType();

                    // Pusj a byte giving the size of the array
                    CodeMethodInvokeExpression pushSize = new CodeMethodInvokeExpression(
                        new CodeVariableReferenceExpression("m"),
                        "WriteByte",
                        new CodeCastExpression(typeof(System.Byte), new CodeFieldReferenceExpression(

                            new CodeFieldReferenceExpression(
                                new CodeVariableReferenceExpression("pkt"),
                                p.Name
                            ),
                            "Length"
                        ))
                    );

                    m.Statements.Add(pushSize);

                    // Push each index of the array
                    // Declares and initializes an integer variable named testInt.
                    string varName = "i_" + p.Name;
                    CodeVariableDeclarationStatement looper = new CodeVariableDeclarationStatement(typeof(int), varName, new CodePrimitiveExpression(0));
                    m.Statements.Add(looper);
                    // Creates a for loop that sets testInt to 0 and continues incrementing testInt by 1 each loop until testInt is not less than 10.
                    CodeIterationStatement forLoop = new CodeIterationStatement(
                        // initStatement parameter for pre-loop initialization.
                        new CodeAssignStatement(new CodeVariableReferenceExpression(varName), new CodePrimitiveExpression(0)),
                        // testExpression parameter to test for continuation condition.
                        new CodeBinaryOperatorExpression(new CodeVariableReferenceExpression(varName),
                            CodeBinaryOperatorType.LessThan, new CodeFieldReferenceExpression(
                            new CodeFieldReferenceExpression(
                                new CodeVariableReferenceExpression("pkt"),
                                p.Name
                            ),
                            "Length"
                        )),
                        // incrementStatement parameter indicates statement to execute after each iteration.
                        new CodeAssignStatement(new CodeVariableReferenceExpression(varName), new CodeBinaryOperatorExpression(
                            new CodeVariableReferenceExpression(varName), CodeBinaryOperatorType.Add, new CodePrimitiveExpression(1))),
                        // statements parameter contains the statements to execute during each interation of the loop.
                        // Each loop iteration the value of the integer is output using the Console.WriteLine method.
                        new CodeStatement[] {
                            new CodeVariableDeclarationStatement(
                                "System.Byte[]",
                                 "byteArr",
                                 new CodeMethodInvokeExpression(
                                     new CodeTypeReferenceExpression("Data_serializers_const." + GetSerializerForType(p)),
                                     // new CodeThisReferenceExpression(),
                                     "Serialize",
                                     new CodeExpression[]  {
                                            new CodeArrayIndexerExpression(
                                                 new CodeFieldReferenceExpression(
                                                         new CodeArgumentReferenceExpression("pkt"),
                                                         p.Name
                                                 ),
                                                 new CodeVariableReferenceExpression(varName)
                                            )
                                     }
                                 )
                            ),
                             new CodeExpressionStatement( new CodeMethodInvokeExpression(
                                new CodeArgumentReferenceExpression("m"),
                                "Write",
                                new CodeExpression[]
                                {
                                    new CodeVariableReferenceExpression("byteArr"),
                                    new CodePrimitiveExpression(0),
                                    new CodeFieldReferenceExpression(new CodeVariableReferenceExpression("byteArr"), "Length")
                                }
                            ))
                        }
                   );

                    m.Statements.Add(forLoop);

                }
                else if (p.FieldType.IsEnum)
                {
                    CodeVariableDeclarationStatement propDef = new CodeVariableDeclarationStatement
                    ("System.Byte[]",
                    p.Name,
                    new CodeMethodInvokeExpression(
                        new CodeTypeReferenceExpression("Data_serializers_const.ser_Int32"),
                        // new CodeThisReferenceExpression(),
                        "Serialize",
                        new CodeExpression[]  {
                                new CodeCastExpression(
                                    "System.Int32", 
                                     new CodeFieldReferenceExpression(
                                        new CodeArgumentReferenceExpression("pkt"),
                                        p.Name
                                )    )
                        }
                        )
                    );
                    m.Statements.Add(propDef);
                    CodeMethodInvokeExpression propSend = new CodeMethodInvokeExpression(
                        new CodeArgumentReferenceExpression("m"),
                        "Write",
                        new CodeExpression[]
                        {
                        new CodeVariableReferenceExpression(p.Name),
                        new CodePrimitiveExpression(0),
                        new CodeFieldReferenceExpression(new CodeVariableReferenceExpression(p.Name), "Length")
                        }
                    );

                    m.Statements.Add(propSend);
                } else
                {
                    CodeVariableDeclarationStatement propDef = new CodeVariableDeclarationStatement
                    ("System.Byte[]",
                    p.Name,
                    new CodeMethodInvokeExpression(
                        new CodeTypeReferenceExpression("Data_serializers_const." + GetSerializerForType(p)),
                        // new CodeThisReferenceExpression(),
                        "Serialize",
                        new CodeExpression[]  {
                                new CodeFieldReferenceExpression(
                                        new CodeArgumentReferenceExpression("pkt"),
                                        p.Name
                                )
                        }
                        )
                    );
                    m.Statements.Add(propDef);
                    CodeMethodInvokeExpression propSend = new CodeMethodInvokeExpression(
                        new CodeArgumentReferenceExpression("m"),
                        "Write",
                        new CodeExpression[]
                        {
                        new CodeVariableReferenceExpression(p.Name),
                        new CodePrimitiveExpression(0),
                        new CodeFieldReferenceExpression(new CodeVariableReferenceExpression(p.Name), "Length")
                        }
                    );

                    m.Statements.Add(propSend);
                }


            }
            programClass.Members.Add(m);

            // Write the deserialize method  // Write the serialize method
            CodeMemberMethod deserialize = new CodeMemberMethod();
            deserialize.Name = "_Deserialize";
            deserialize.Attributes = MemberAttributes.Public | MemberAttributes.Final;

            deserialize.Parameters.Add(new CodeParameterDeclarationExpression(t.Name, "pkt"));
            deserialize.Parameters.Add(new CodeParameterDeclarationExpression("MemoryStream", "m"));

            props = t.GetFields();
            Debug.Log("Type name is " + t.Name);
            // .Where(propertyInfo => propertyInfo.GetCustomAttributes(false).GetType() != typeof(LiteLocalOnly));
            foreach (FieldInfo p in props)
            {
                if (p.FieldType.IsArray)
                {
                    Type subType = p.FieldType.GetElementType();

                    // Pop a byte giving the size of the array
                    CodeVariableDeclarationStatement pushSize = new CodeVariableDeclarationStatement(
                        "System.Int32",
                        "length_" + p.Name,
                        new CodeMethodInvokeExpression(
                            new CodeVariableReferenceExpression("m"),
                            "ReadByte"
                        )
                    );
                    deserialize.Statements.Add(pushSize);

                    // Init the array
                    CodeAssignStatement createNewArray = new CodeAssignStatement(
                        new CodeFieldReferenceExpression(new CodeVariableReferenceExpression("pkt"), p.Name),
                        new CodeArrayCreateExpression(
                            subType.Name,
                            new CodeVariableReferenceExpression("length_" + p.Name)
                        )
                    );
                    deserialize.Statements.Add(createNewArray);

                    // Push each index of the array
                    // Declares and initializes an integer variable named testInt.
                    string varName = "i_" + p.Name;
                    CodeVariableDeclarationStatement looper = new CodeVariableDeclarationStatement(typeof(int), varName, new CodePrimitiveExpression(0));
                    deserialize.Statements.Add(looper);
                    // Creates a for loop that sets testInt to 0 and continues incrementing testInt by 1 each loop until testInt is not less than 10.
                    CodeIterationStatement forLoop = new CodeIterationStatement(
                        // initStatement parameter for pre-loop initialization.
                        new CodeAssignStatement(new CodeVariableReferenceExpression(varName), new CodePrimitiveExpression(0)),
                        // testExpression parameter to test for continuation condition.
                        new CodeBinaryOperatorExpression(new CodeVariableReferenceExpression(varName),
                            CodeBinaryOperatorType.LessThan, new CodeVariableReferenceExpression("length_" + p.Name)),
                        // incrementStatement parameter indicates statement to execute after each iteration.
                        new CodeAssignStatement(new CodeVariableReferenceExpression(varName), new CodeBinaryOperatorExpression(
                            new CodeVariableReferenceExpression(varName), CodeBinaryOperatorType.Add, new CodePrimitiveExpression(1))),
                        // statements parameter contains the statements to execute during each interation of the loop.
                        // Each loop iteration the value of the integer is output using the Console.WriteLine method.
                        new CodeStatement[] {
                            new CodeAssignStatement(
                                new CodeArrayIndexerExpression(
                                    new CodeFieldReferenceExpression(
                                        new CodeVariableReferenceExpression("pkt"),
                                        p.Name
                                    ),

                                    new CodeVariableReferenceExpression(varName)
                                ),
                                new CodeMethodInvokeExpression(
                                    new CodeTypeReferenceExpression("Data_serializers_const." + GetSerializerForType(p)),
                                    // new CodeThisReferenceExpression(),
                                    "Deserialize",
                                    new CodeExpression[]  {
                                            new CodeArgumentReferenceExpression("m")
                                    }
                                )
                             )
                        }
                   );

                    deserialize.Statements.Add(forLoop);

                }else if (p.FieldType.IsEnum)
                {
                    CodeAssignStatement assign = new CodeAssignStatement(
                        new CodeFieldReferenceExpression(new CodeArgumentReferenceExpression("pkt"), p.Name),
                        new CodeCastExpression(p.FieldType, new CodeMethodInvokeExpression(
                            new CodeTypeReferenceExpression("Data_serializers_const.ser_Int32"),
                            // new CodeThisReferenceExpression(),
                            "Deserialize",
                            new CodeExpression[]  {
                                new CodeArgumentReferenceExpression("m")
                            }
                        ))

                    );
                }
                else
                {
                    CodeAssignStatement assign = new CodeAssignStatement(
                        new CodeFieldReferenceExpression(new CodeArgumentReferenceExpression("pkt"), p.Name),
                        new CodeMethodInvokeExpression(
                            new CodeTypeReferenceExpression("Data_serializers_const." + GetSerializerForType(p)),
                            // new CodeThisReferenceExpression(),
                            "Deserialize",
                            new CodeExpression[]  {
                                new CodeArgumentReferenceExpression("m")
                            }
                        )

                    );
                    deserialize.Statements.Add(assign);
                }

            }
            programClass.Members.Add(deserialize);

            // Write the fire method
            CodeMemberMethod fire = new CodeMemberMethod();
            fire.Name = "Fire";
            fire.Attributes = MemberAttributes.Override | MemberAttributes.Public;

            fire.Parameters.Add(new CodeParameterDeclarationExpression("MemoryStream", "m"));

            fire.Statements.Add(
                new CodeMethodInvokeExpression(
                    new CodeThisReferenceExpression(),
                    "_Deserialize",
                    new CodeExpression[]
                    {
                        new CodeFieldReferenceExpression(new CodeVariableReferenceExpression("ConstRefs"), "pkt_" + t.Name),
                        new CodeVariableReferenceExpression("m")
                    }
                )
            );

            fire.Statements.Add(
                new CodeMethodInvokeExpression(
                    //new CodeVariableReferenceExpression("pkt"),
                    new CodeFieldReferenceExpression(new CodeVariableReferenceExpression("ConstRefs"), "pkt_" + t.Name),
                    "Execute"
                )
            );
            programClass.Members.Add(fire);

            // Add it to the generator
            nmspc.Types.Add(programClass);

        }

        public static void FinalizeGenerator()
        {
            CodeDomProvider codeDomProvider = CodeDomProvider.CreateProvider("CSharp");
            CodeGeneratorOptions generatorOptions = new CodeGeneratorOptions { BracingStyle = "C" };
            using (StreamWriter sourceWriter = new StreamWriter("main.cs"))
            {
                codeDomProvider.GenerateCodeFromCompileUnit(ccu, sourceWriter, generatorOptions);
            }

        }
    }
  

}