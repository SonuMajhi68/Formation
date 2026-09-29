using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Text;
using System.Text.RegularExpressions;
using UnityEditor;
using UnityEditor.Rendering;
using UnityEngine;

namespace Formation.ArmOCBridge
{
    public class CompiledShaderResult
    {
        public string pass;
        //public string keywords;
        public List<string> keywords;
        public string vertexShader;
        public string fragmentShader;
    }

    public static class ArmOCBridgeShaderCompiler
    {
        public static List<CompiledShaderResult> ExtractAllVariants(Shader shader)
        {
            var variants = new List<CompiledShaderResult>();
            if (shader == null) return variants;

            string compiled = CompileReadShader(shader);
            if (string.IsNullOrEmpty(compiled)) return variants;

            // Sequential Scan
            string vertCode = "";
            string fragCode = "";
            string currentPassName = "";
            List<String> currentKeywords = new List<string>();


            string[] lines = compiled.Split(new[] { "\r\n", "\n", "\r" }, StringSplitOptions.None);
            for (int i = 0; i < lines.Length; i++)
            {
                string line = lines[i];

                // Extract the Pass
                var mName = Regex.Match(line, @"^\s*Name\s*""([^""]+)""", RegexOptions.IgnoreCase);
                if (mName.Success)
                {
                    //Debug.Log("2");
                    currentPassName = mName.Groups[1].Value.Trim();
                    continue;
                }

                // Extract the Keywords
                var mKw = Regex.Match(line, @"^\s*Keywords\s*:\s*(.+)$", RegexOptions.IgnoreCase);
                if (mKw.Success)
                {
                    currentKeywords = new List<string>(mKw.Groups[1].Value.Trim().Split(' '));
                    continue;
                }

                // Extract the Vertex Disassembly
                if (line.Trim() == "#ifdef VERTEX")
                {
                    string code = CollectShaderSection(lines, ref i, "#ifdef VERTEX", "#endif");
                    vertCode = ProcessGLSLVersion(code);

                    continue;
                }

                // Extract the Fragment Disassembly
                if (line.Trim() == "#ifdef FRAGMENT")
                {
                    string code = CollectShaderSection(lines, ref i, "#ifdef FRAGMENT", "#endif");
                    fragCode = ProcessGLSLVersion(code);

                    continue;
                }

                if (vertCode != "" && fragCode != "")
                {
                    CompiledShaderResult val = new CompiledShaderResult();
                    val.pass = (currentPassName != "") ? currentPassName : "<unnamed>";
                    val.keywords = currentKeywords;
                    val.vertexShader = vertCode;
                    val.fragmentShader = fragCode;

                    variants.Add(val);

                    vertCode = "";
                    fragCode = "";
                }
            }

            return variants;
        }



        static string CollectShaderSection(string[] allLines, ref int index, string start, string end)
        {
            var sb = new StringBuilder();
            int depth = 0;

            for (int i = index + 1; i < allLines.Length; i++)
            {
                string l = allLines[i].Trim();
                if (l.StartsWith("#if") || l.StartsWith("#ifdef") || l.StartsWith("#ifndef"))
                {
                    depth++;
                    sb.AppendLine(allLines[i]);
                    continue;
                }
                if (l == end)
                {
                    if (depth == 0)
                    {
                        index = i; // Consume the outer #endif.
                        break;
                    }
                    else
                    {
                        depth--;
                        sb.AppendLine(allLines[i]);
                        continue;
                    }
                }
                sb.AppendLine(allLines[i]);
            }
            return sb.ToString().TrimEnd('\r', '\n');
        }



        private static string ProcessGLSLVersion(string shaderCode)
        {
            if (string.IsNullOrEmpty(shaderCode))
                return shaderCode;

            // #version 300 es -> #version 310 es
            return Regex.Replace(shaderCode, @"#version\s+300\s+es", "#version 310 es");
        }



        private static string CompileReadShader(Shader shader)
        {
            try
            {
                bool success = InvokeOpenCompiledShader(shader);
                if (success)
                {
                    //Application.dataPath      ->      C:/ Project / Unity / Learning_Shader / Assets
                    string targetPath = Application.dataPath + "/../Temp/Compiled-" + shader.name.Replace("/", "-") + ".shader";
                    if (File.Exists(targetPath))
                    {
                        return File.ReadAllText(targetPath);
                    }
                    return string.Empty;
                }
                return string.Empty;
            }
            catch (System.Exception e)
            {
                Debug.LogError($"Error compiling shader: {e.Message}");
                return null;
            }
        }



        private static bool InvokeOpenCompiledShader(Shader shader)
        {
            int mode = 3;                                                                               // Mode：Custom
            int externPlatformsMask = 1 << (int)UnityEditor.Rendering.ShaderCompilerPlatform.GLES3x;    // Target platform mask（Windows）
            bool includeAllVariants = true;                                                             // Contains all variations
            bool preprocessOnly = false;                                                                // Not only preprocessing
            bool stripLineDirectives = false;                                                           // Unstripped line instructions

            if (shader == null)
            {
                Debug.LogError("Shader cannot be null");
                return false;
            }

            try
            {
                // Get ShaderUtil type
                Type shaderUtilType = typeof(ShaderUtil);

                // Find the OpenCompiledShader method
                MethodInfo openCompiledMethod = shaderUtilType.GetMethod(
                    "OpenCompiledShader",
                    BindingFlags.Static | BindingFlags.NonPublic, // internal static
                    null,
                    new Type[] {
                    typeof(Shader),               // shader
                    typeof(int),                  // mode
                    typeof(int),                  // externPlatformsMask
                    typeof(bool),                 // includeAllVariants
                    typeof(bool),                 // preprocessOnly
                    typeof(bool)                  // stripLineDirectives
                    },
                    null
                );

                if (openCompiledMethod == null)
                {
                    Debug.LogError("Could not find OpenCompiledShader method");
                    return false;
                }

                openCompiledMethod.Invoke(
                    null,                           // Static methods, the first parameter is null.
                    new object[] {
                    shader,
                    mode,
                    externPlatformsMask,
                    includeAllVariants,
                    preprocessOnly,
                    stripLineDirectives
                    }
                );

                return true;
            }
            catch (TargetInvocationException e)
            {
                Debug.LogError($"Error invoking OpenCompiledShader: {e.InnerException.Message}");
                return false;
            }
            catch (Exception e)
            {
                Debug.LogError($"Reflection error: {e.Message}");
                return false;
            }
        }
    }

}
