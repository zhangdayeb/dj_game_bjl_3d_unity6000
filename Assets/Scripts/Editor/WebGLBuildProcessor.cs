using UnityEditor;
using UnityEditor.Build;
using UnityEditor.Build.Reporting;
using UnityEngine;
using System.IO;
using System.Text;
using System.Collections.Generic;
using UnityEditor.WebGL;
using UnityEngine.Rendering; // 🔥 添加这行

namespace BaccaratGame.Editor
{
    /// <summary>
    /// WebGL构建处理器 - 自动优化WebGL构建设置和处理构建流程
    /// </summary>
    public class WebGLBuildProcessor : IPreprocessBuildWithReport, IPostprocessBuildWithReport
    {
        // 构建优先级
        public int callbackOrder => 1;
        
        // 构建配置
        private static readonly Dictionary<string, object> BuildSettings = new Dictionary<string, object>
        {
            { "CompanyName", "SlotMachine Studio" },
            { "ProductName", "Slot Machine Game" },
            { "Version", "1.0.0" },
            { "BundleVersion", "1" }
        };
        
        // Safari优化设置
        private static readonly Dictionary<string, bool> SafariOptimizations = new Dictionary<string, bool>
        {
            { "DisableWebAssemblyStreaming", true },
            { "EnableMemoryOptimization", true },
            { "DisableIndexedDB", false },
            { "EnableAudioWorklet", false },
            { "UseUnityHeap", true }
        };
        
        #region IPreprocessBuildWithReport Implementation
        
        /// <summary>
        /// 构建前预处理
        /// </summary>
        public void OnPreprocessBuild(BuildReport report)
        {
            Debug.Log("[WebGLBuildProcessor] 开始WebGL构建预处理...");
            
            if (report.summary.platform != BuildTarget.WebGL)
            {
                return;
            }
            
            try
            {
                // 应用WebGL优化设置
                ApplyWebGLOptimizations();
                
                // 配置播放器设置
                ConfigurePlayerSettings();
                
                // 优化图形设置
                OptimizeGraphicsSettings();
                
                // 配置音频设置
                ConfigureAudioSettings();
                
                // 处理流式资源
                ProcessStreamingAssets();
                
                // 验证WebGL模板
                ValidateWebGLTemplate();
                
                Debug.Log("[WebGLBuildProcessor] WebGL构建预处理完成");
            }
            catch (System.Exception e)
            {
                Debug.LogError($"[WebGLBuildProcessor] 预处理失败: {e.Message}");
                throw;
            }
        }
        
        #endregion
        
        #region IPostprocessBuildWithReport Implementation
        
        /// <summary>
        /// 构建后处理
        /// </summary>
        public void OnPostprocessBuild(BuildReport report)
        {
            Debug.Log("[WebGLBuildProcessor] 开始WebGL构建后处理...");
            
            if (report.summary.platform != BuildTarget.WebGL)
            {
                return;
            }
            
            try
            {
                string buildPath = report.summary.outputPath;
                
                // 优化构建输出
                OptimizeBuildOutput(buildPath);
                
                // 注入Safari兼容性代码
                InjectSafariCompatibility(buildPath);
                
                // 🔥 新增：注入iframe功能
                InjectIframeSupport(buildPath);
                
                // 创建配置文件
                CreateConfigurationFiles(buildPath);
                
                // 生成构建报告
                GenerateBuildReport(report, buildPath);
                
                // 验证构建结果
                ValidateBuildOutput(buildPath);
                
                Debug.Log($"[WebGLBuildProcessor] WebGL构建后处理完成. 输出路径: {buildPath}");
            }
            catch (System.Exception e)
            {
                Debug.LogError($"[WebGLBuildProcessor] 后处理失败: {e.Message}");
            }
        }
        
        #endregion
        
        #region WebGL Optimizations
        
        /// <summary>
        /// 应用WebGL优化设置 - 🔥 移动端优化版本
        /// </summary>
        private static void ApplyWebGLOptimizations()
        {
            Debug.Log("[WebGLBuildProcessor] 应用移动端WebGL优化设置...");
            
            // 🔥 移动端内存优化：从512MB改为128MB
            PlayerSettings.WebGL.memorySize = 128; 
            
            // 启用异常支持（用于调试）
            PlayerSettings.WebGL.exceptionSupport = WebGLExceptionSupport.ExplicitlyThrownExceptionsOnly;
            
            // 设置压缩格式
            PlayerSettings.WebGL.compressionFormat = WebGLCompressionFormat.Gzip;
            
            // 启用数据缓存
            PlayerSettings.WebGL.dataCaching = true;
            
            // 🔥 关键修复：禁用文件名哈希，解决文件名问题
            PlayerSettings.WebGL.nameFilesAsHashes = false;
            
            // 设置代码优化
            PlayerSettings.WebGL.debugSymbolMode = WebGLDebugSymbolMode.Off;
            
            // 🔥 移除重复的压缩格式设置
            // PlayerSettings.WebGL.compressionFormat = WebGLCompressionFormat.Gzip; // 删除重复行
            
            Debug.Log("[WebGLBuildProcessor] 移动端WebGL优化设置应用完成");
            Debug.Log($"[WebGLBuildProcessor] ✅ Name Files As Hashes: {PlayerSettings.WebGL.nameFilesAsHashes}");
            Debug.Log($"[WebGLBuildProcessor] ✅ Memory Size: {PlayerSettings.WebGL.memorySize} MB");
        }
        
        /// <summary>
        /// 配置播放器设置 - 🔥 移动端适配
        /// </summary>
        private static void ConfigurePlayerSettings()
        {
            Debug.Log("[WebGLBuildProcessor] 配置移动端播放器设置...");
            
            // 基础设置
            PlayerSettings.companyName = BuildSettings["CompanyName"].ToString();
            PlayerSettings.productName = BuildSettings["ProductName"].ToString();
            PlayerSettings.bundleVersion = BuildSettings["Version"].ToString();
            
            // 🔥 移动端分辨率设置：改为竖屏750x1344
            PlayerSettings.defaultScreenWidth = 750;
            PlayerSettings.defaultScreenHeight = 1344;
            PlayerSettings.runInBackground = true;
            
            // 配置设置
            PlayerSettings.stripEngineCode = true;
            PlayerSettings.stripUnusedMeshComponents = true;
            
            // 脚本定义符号
            var namedBuildTarget = NamedBuildTarget.FromBuildTargetGroup(BuildTargetGroup.WebGL);
            string currentSymbols = PlayerSettings.GetScriptingDefineSymbols(namedBuildTarget);
            HashSet<string> symbolSet = new HashSet<string>(currentSymbols.Split(';'));

            symbolSet.Add("WEBGL_BUILD");
            symbolSet.Add("SLOT_MACHINE");
            symbolSet.Add("MOBILE_OPTIMIZED"); // 🔥 新增移动端标识

            #if UNITY_2023_1_OR_NEWER
            symbolSet.Add("UNITY_2023_OR_NEWER");
            #endif

            string newSymbols = string.Join(";", symbolSet);
            PlayerSettings.SetScriptingDefineSymbols(namedBuildTarget, newSymbols);
            
            Debug.Log($"[WebGLBuildProcessor] 脚本定义符号: {newSymbols}");
            Debug.Log($"[WebGLBuildProcessor] ✅ 移动端分辨率: {PlayerSettings.defaultScreenWidth} x {PlayerSettings.defaultScreenHeight}");
        }
        
        /// <summary>
        /// 优化图形设置
        /// </summary>
        private static void OptimizeGraphicsSettings()
        {
            Debug.Log("[WebGLBuildProcessor] 优化图形设置...");
            
            // 获取当前图形设置
            var graphicsSettings = UnityEngine.Rendering.GraphicsSettings.currentRenderPipeline;
            
            // 设置色彩空间
            PlayerSettings.colorSpace = ColorSpace.Linear;
            
            // 禁用不必要的图形API
            PlayerSettings.SetUseDefaultGraphicsAPIs(BuildTarget.WebGL, false);
            PlayerSettings.SetGraphicsAPIs(BuildTarget.WebGL, new UnityEngine.Rendering.GraphicsDeviceType[] 
            { 
                UnityEngine.Rendering.GraphicsDeviceType.OpenGLES3
            });
            
            Debug.Log("[WebGLBuildProcessor] 图形设置优化完成");
        }
        
        /// <summary>
        /// 配置音频设置
        /// </summary>
        private static void ConfigureAudioSettings()
        {
            Debug.Log("[WebGLBuildProcessor] 配置音频设置...");
            
            // 禁用不需要的音频设置
            var config = AudioSettings.GetConfiguration();
            config.dspBufferSize = 1024; // 优化音频延迟
            config.numRealVoices = 32;
            config.numVirtualVoices = 512;
            config.sampleRate = 44100;
            AudioSettings.Reset(config);
            
            Debug.Log("[WebGLBuildProcessor] 音频设置配置完成");
        }
        
        #endregion
        
        #region Build Output Optimization
        
        /// <summary>
        /// 优化构建输出
        /// </summary>
        private static void OptimizeBuildOutput(string buildPath)
        {
            Debug.Log("[WebGLBuildProcessor] 优化构建输出...");
            
            try
            {
                // 压缩资源文件
                CompressBuildAssets(buildPath);
                
                // 优化加载性能
                OptimizeLoadingPerformance(buildPath);
                
                // 清理不必要的文件
                CleanupBuildFiles(buildPath);
                
                Debug.Log("[WebGLBuildProcessor] 构建输出优化完成");
            }
            catch (System.Exception e)
            {
                Debug.LogError($"[WebGLBuildProcessor] 构建输出优化失败: {e.Message}");
            }
        }
        
        /// <summary>
        /// 压缩构建资源
        /// </summary>
        private static void CompressBuildAssets(string buildPath)
        {
            Debug.Log("[WebGLBuildProcessor] 压缩构建资源...");
            
            string buildDataPath = Path.Combine(buildPath, "Build");
            if (!Directory.Exists(buildDataPath))
            {
                Debug.LogWarning($"[WebGLBuildProcessor] 构建数据路径不存在: {buildDataPath}");
                return;
            }
            
            // 这里可以添加额外的压缩逻辑
            // 例如：使用第三方压缩工具进一步压缩文件
        }
        
        /// <summary>
        /// 优化加载性能 - 🔥 修复文件名引用
        /// </summary>
        private static void OptimizeLoadingPerformance(string buildPath)
        {
            Debug.Log("[WebGLBuildProcessor] 优化加载性能...");
            
            string indexPath = Path.Combine(buildPath, "index.html");
            if (File.Exists(indexPath))
            {
                string content = File.ReadAllText(indexPath);
                
                // 🔥 修复预加载提示，使用正确的文件名
                content = content.Replace("</head>", 
                    "  <link rel=\"preload\" as=\"script\" href=\"Build/build.loader.js\">\n" +
                    "  <link rel=\"preload\" as=\"fetch\" href=\"Build/build.wasm\" crossorigin>\n" +
                    "  <meta name=\"viewport\" content=\"width=device-width, initial-scale=1.0, user-scalable=no\">\n" +
                    "</head>");
                
                File.WriteAllText(indexPath, content);
                Debug.Log("[WebGLBuildProcessor] 添加预加载优化（使用固定文件名）");
            }
        }
        
        /// <summary>
        /// 清理构建文件
        /// </summary>
        private static void CleanupBuildFiles(string buildPath)
        {
            Debug.Log("[WebGLBuildProcessor] 清理构建文件...");
            
            // 删除开发相关的文件
            string[] filesToDelete = { 
                "*.symbols.json",
                "development_build_info.json"
            };
            
            foreach (string pattern in filesToDelete)
            {
                string[] files = Directory.GetFiles(buildPath, pattern, SearchOption.AllDirectories);
                foreach (string file in files)
                {
                    try
                    {
                        File.Delete(file);
                        Debug.Log($"[WebGLBuildProcessor] 删除文件: {file}");
                    }
                    catch (System.Exception e)
                    {
                        Debug.LogWarning($"[WebGLBuildProcessor] 无法删除文件 {file}: {e.Message}");
                    }
                }
            }
        }
        
        #endregion
        
        #region Safari Compatibility
        
        /// <summary>
        /// 注入Safari兼容性代码
        /// </summary>
        private static void InjectSafariCompatibility(string buildPath)
        {
            Debug.Log("[WebGLBuildProcessor] 注入Safari兼容性代码...");
            
            try
            {
                string indexPath = Path.Combine(buildPath, "index.html");
                if (!File.Exists(indexPath))
                {
                    Debug.LogWarning($"[WebGLBuildProcessor] index.html不存在: {indexPath}");
                    return;
                }
                
                string content = File.ReadAllText(indexPath);
                
                // 注入Safari兼容性脚本
                string safariScript = GenerateSafariCompatibilityScript();
                content = content.Replace("</body>", $"{safariScript}\n</body>");
                
                File.WriteAllText(indexPath, content);
                
                Debug.Log("[WebGLBuildProcessor] Safari兼容性代码注入完成");
            }
            catch (System.Exception e)
            {
                Debug.LogError($"[WebGLBuildProcessor] Safari兼容性代码注入失败: {e.Message}");
            }
        }
        
        /// <summary>
        /// 🔥 新增：注入iframe支持功能
        /// </summary>
        private static void InjectIframeSupport(string buildPath)
        {
            Debug.Log("[WebGLBuildProcessor] 注入iframe支持功能...");
            
            try
            {
                string indexPath = Path.Combine(buildPath, "index.html");
                if (!File.Exists(indexPath))
                {
                    Debug.LogWarning($"[WebGLBuildProcessor] index.html不存在: {indexPath}");
                    return;
                }
                
                string content = File.ReadAllText(indexPath);
                
                // 注入iframe支持脚本
                string iframeScript = GenerateIframeSupportScript();
                content = content.Replace("</body>", $"{iframeScript}\n</body>");
                
                File.WriteAllText(indexPath, content);
                
                Debug.Log("[WebGLBuildProcessor] iframe支持功能注入完成");
            }
            catch (System.Exception e)
            {
                Debug.LogError($"[WebGLBuildProcessor] iframe支持功能注入失败: {e.Message}");
            }
        }
        
        /// <summary>
        /// 生成Safari兼容性脚本
        /// </summary>
        private static string GenerateSafariCompatibilityScript()
        {
            return @"
<script>
// Safari兼容性修复
(function() {
    'use strict';
    
    // 检测Safari浏览器
    var isSafari = /^((?!chrome|android).)*safari/i.test(navigator.userAgent);
    
    if (isSafari) {
        console.log('检测到Safari浏览器，应用兼容性修复...');
        
        // 禁用WebAssembly流式编译
        if (window.WebAssembly && window.WebAssembly.instantiateStreaming) {
            delete window.WebAssembly.instantiateStreaming;
        }
        
        // 音频上下文修复
        window.addEventListener('touchstart', function safariAudioFix() {
            var audioContext = window.AudioContext || window.webkitAudioContext;
            if (audioContext) {
                var ctx = new audioContext();
                var buf = ctx.createBuffer(1, 1, 22050);
                var src = ctx.createBufferSource();
                src.buffer = buf;
                src.connect(ctx.destination);
                src.start(0);
                ctx.close();
            }
            window.removeEventListener('touchstart', safariAudioFix);
        }, false);
        
        // 内存管理优化
        setInterval(function() {
            if (window.gc) {
                window.gc();
            }
        }, 30000);
    }
    
    // 通用WebGL优化
    window.addEventListener('beforeunload', function() {
        if (window.unityInstance) {
            window.unityInstance.Quit();
        }
    });
})();
</script>";
        }
        
        /// <summary>
        /// 🔥 新增：生成iframe支持脚本
        /// </summary>
        private static string GenerateIframeSupportScript()
        {
            return @"
<script>
// iframe 管理功能 - 移动端优化版
(function() {
    'use strict';
    
    /**
     * 通用 iframe 加载函数
     * Unity 调用: CallWebGLFunction('loadIframe', 'containerID,URL')
     */
    window.loadIframe = function(containerIdAndUrl) {
        try {
            console.log('🎯 loadIframe 调用:', containerIdAndUrl);
            
            var parts = containerIdAndUrl.split(',');
            var containerId = parts[0];
            var url = parts.slice(1).join(',');
            
            var container = document.getElementById(containerId);
            if (!container) {
                container = document.createElement('div');
                container.id = containerId;
                container.style.cssText = 
                    'position: fixed; top: 0; left: 0; width: 100%; height: 100%; ' +
                    'z-index: 2000; background: rgba(0,0,0,0.9); ' +
                    'display: flex; justify-content: center; align-items: center;';
                document.body.appendChild(container);
            }
            
            var wrapper = document.createElement('div');
            wrapper.style.cssText = 
                'width: 100%; height: 100%; max-width: 750px; max-height: 1344px; ' +
                'position: relative; background: #000; border-radius: 8px; overflow: hidden;';
            
            var closeBtn = document.createElement('button');
            closeBtn.innerHTML = '✕';
            closeBtn.style.cssText = 
                'position: absolute; top: 10px; right: 10px; width: 40px; height: 40px; ' +
                'background: rgba(255,0,0,0.8); color: white; border: none; ' +
                'border-radius: 50%; cursor: pointer; z-index: 2001; font-size: 16px; ' +
                'transition: all 0.3s;';
            
            closeBtn.onclick = function() {
                console.log('🚫 关闭iframe:', containerId);
                if (container && container.parentNode) {
                    container.parentNode.removeChild(container);
                }
            };
            
            var iframe = document.createElement('iframe');
            iframe.src = url;
            iframe.style.cssText = 'width:100%;height:100%;border:none;';
            iframe.setAttribute('allowfullscreen', 'true');
            iframe.setAttribute('frameborder', '0');
            iframe.setAttribute('scrolling', 'no');
            
            container.innerHTML = '';
            wrapper.appendChild(iframe);
            wrapper.appendChild(closeBtn);
            container.appendChild(wrapper);
            
            console.log('✅ iframe 加载成功:', containerId, url);
            return true;
        } catch (e) {
            console.error('❌ loadIframe 失败:', e);
            return false;
        }
    };
    
    /**
     * 设置 iframe 可见性
     */
    window.setIframeVisibility = function(containerIdAndVisibility) {
        try {
            var parts = containerIdAndVisibility.split(',');
            var containerId = parts[0];
            var visible = parts[1] === '1';
            
            var container = document.getElementById(containerId);
            if (container) {
                container.style.display = visible ? 'flex' : 'none';
                console.log('🔄 iframe 可见性:', containerId, visible ? '显示' : '隐藏');
                return true;
            }
            return false;
        } catch (e) {
            console.error('❌ setIframeVisibility 失败:', e);
            return false;
        }
    };
    
    /**
     * 刷新 iframe
     */
    window.refreshIframe = function(containerId) {
        try {
            var container = document.getElementById(containerId);
            if (container) {
                var iframe = container.querySelector('iframe');
                if (iframe) {
                    var currentSrc = iframe.src;
                    iframe.src = '';
                    setTimeout(function() { iframe.src = currentSrc; }, 100);
                    console.log('🔄 iframe 已刷新:', containerId);
                    return true;
                }
            }
            return false;
        } catch (e) {
            console.error('❌ refreshIframe 失败:', e);
            return false;
        }
    };
    
    /**
     * 销毁 iframe
     */
    window.destroyIframe = function(containerId) {
        try {
            var container = document.getElementById(containerId);
            if (container && container.parentNode) {
                container.parentNode.removeChild(container);
                console.log('🗑️ iframe 已销毁:', containerId);
                return true;
            }
            return false;
        } catch (e) {
            console.error('❌ destroyIframe 失败:', e);
            return false;
        }
    };
    
    // ESC 键关闭功能
    document.addEventListener('keydown', function(event) {
        if (event.key === 'Escape') {
            var containers = document.querySelectorAll('[id$=""-container""]');
            if (containers.length > 0) {
                var latestContainer = containers[containers.length - 1];
                if (latestContainer.style.display !== 'none') {
                    latestContainer.style.display = 'none';
                    console.log('⌨️ ESC 关闭iframe:', latestContainer.id);
                }
            }
        }
    });
    
    console.log('🚀 iframe 管理功能已加载');
    console.log('📋 可用函数:', ['loadIframe', 'setIframeVisibility', 'refreshIframe', 'destroyIframe']);
})();
</script>";
        }
        
        #endregion
        
        #region Configuration and Validation
        
        /// <summary>
        /// 处理流式资源
        /// </summary>
        private static void ProcessStreamingAssets()
        {
            Debug.Log("[WebGLBuildProcessor] 处理流式资源...");
            
            string streamingAssetsPath = Application.streamingAssetsPath;
            if (!Directory.Exists(streamingAssetsPath))
            {
                Directory.CreateDirectory(streamingAssetsPath);
            }
            
            // 创建WebGL配置文件
            string configPath = Path.Combine(streamingAssetsPath, "webgl-config.json");
            var config = new
            {
                platform = "WebGL",
                version = Application.version,
                buildTime = System.DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"),
                safariOptimizations = SafariOptimizations,
                memorySettings = new
                {
                    initialHeapSize = PlayerSettings.WebGL.memorySize,
                    maximumHeapSize = PlayerSettings.WebGL.memorySize * 2
                },
                // 🔥 新增移动端配置
                mobileSettings = new
                {
                    screenWidth = PlayerSettings.defaultScreenWidth,
                    screenHeight = PlayerSettings.defaultScreenHeight,
                    nameFilesAsHashes = PlayerSettings.WebGL.nameFilesAsHashes
                }
            };
            
            string configJson = JsonUtility.ToJson(config, true);
            File.WriteAllText(configPath, configJson);
            
            Debug.Log($"[WebGLBuildProcessor] WebGL配置文件已创建: {configPath}");
        }
        
        /// <summary>
        /// 验证WebGL模板
        /// </summary>
        private static void ValidateWebGLTemplate()
        {
            Debug.Log("[WebGLBuildProcessor] 验证WebGL模板...");
            
            string templatePath = Path.Combine(Application.dataPath, "..", "Assets", "WebGLTemplates");
            if (!Directory.Exists(templatePath))
            {
                Debug.LogWarning("[WebGLBuildProcessor] WebGL模板目录不存在，将使用默认模板");
                return;
            }
            
            // 🔥 检查 MobileTemplate
            string mobileTemplatePath = Path.Combine(templatePath, "MobileTemplate");
            if (Directory.Exists(mobileTemplatePath))
            {
                Debug.Log("[WebGLBuildProcessor] 找到MobileTemplate模板");
                PlayerSettings.WebGL.template = "PROJECT:MobileTemplate";
            }
            // 检查自定义模板
            else
            {
                string customTemplatePath = Path.Combine(templatePath, "SlotMachineTemplate");
                if (Directory.Exists(customTemplatePath))
                {
                    Debug.Log("[WebGLBuildProcessor] 找到自定义WebGL模板");
                    PlayerSettings.WebGL.template = "PROJECT:SlotMachineTemplate";
                }
                else
                {
                    Debug.Log("[WebGLBuildProcessor] 使用默认WebGL模板");
                    PlayerSettings.WebGL.template = "APPLICATION:Default";
                }
            }
        }
        
        /// <summary>
        /// 创建配置文件
        /// </summary>
        private static void CreateConfigurationFiles(string buildPath)
        {
            Debug.Log("[WebGLBuildProcessor] 创建配置文件...");
            
            // 创建.htaccess文件（Apache服务器配置）
            CreateHtaccessFile(buildPath);
            
            // 创建web.config文件（IIS服务器配置）
            CreateWebConfigFile(buildPath);
            
            // 创建nginx配置建议文件
            CreateNginxConfigFile(buildPath);
        }
        
        /// <summary>
        /// 创建.htaccess文件
        /// </summary>
        private static void CreateHtaccessFile(string buildPath)
        {
            string htaccessContent = @"
# Unity WebGL构建的Apache服务器配置
RewriteEngine On

# 启用Gzip压缩
<IfModule mod_deflate.c>
    AddOutputFilterByType DEFLATE text/plain
    AddOutputFilterByType DEFLATE text/html
    AddOutputFilterByType DEFLATE text/xml
    AddOutputFilterByType DEFLATE text/css
    AddOutputFilterByType DEFLATE application/xml
    AddOutputFilterByType DEFLATE application/xhtml+xml
    AddOutputFilterByType DEFLATE application/rss+xml
    AddOutputFilterByType DEFLATE application/javascript
    AddOutputFilterByType DEFLATE application/x-javascript
    AddOutputFilterByType DEFLATE application/octet-stream
</IfModule>

# 设置MIME类型
AddType application/wasm .wasm
AddType application/javascript .js
AddType application/octet-stream .data
AddType application/octet-stream .unityweb

# 缓存策略
<FilesMatch ""\.(wasm|data|unityweb)$"">
    Header set Cache-Control ""max-age=31536000""
</FilesMatch>

# 启用跨域资源共享
Header always set Access-Control-Allow-Origin ""*""
Header always set Access-Control-Allow-Methods ""GET, POST, OPTIONS""
Header always set Access-Control-Allow-Headers ""*""
";
            
            string htaccessPath = Path.Combine(buildPath, ".htaccess");
            File.WriteAllText(htaccessPath, htaccessContent.Trim());
            
            Debug.Log($"[WebGLBuildProcessor] .htaccess文件已创建: {htaccessPath}");
        }
        
        /// <summary>
        /// 创建web.config文件
        /// </summary>
        private static void CreateWebConfigFile(string buildPath)
        {
            string webConfigContent = @"<?xml version=""1.0"" encoding=""UTF-8""?>
<configuration>
    <system.webServer>
        <staticContent>
            <mimeMap fileExtension="".wasm"" mimeType=""application/wasm"" />
            <mimeMap fileExtension="".data"" mimeType=""application/octet-stream"" />
            <mimeMap fileExtension="".unityweb"" mimeType=""application/octet-stream"" />
        </staticContent>
        <httpCompression>
            <dynamicTypes>
                <add mimeType=""application/wasm"" enabled=""true"" />
                <add mimeType=""application/octet-stream"" enabled=""true"" />
            </dynamicTypes>
        </httpCompression>
        <httpHeaders>
            <add name=""Access-Control-Allow-Origin"" value=""*"" />
            <add name=""Access-Control-Allow-Methods"" value=""GET, POST, OPTIONS"" />
            <add name=""Access-Control-Allow-Headers"" value=""*"" />
        </httpHeaders>
    </system.webServer>
</configuration>";
            
            string webConfigPath = Path.Combine(buildPath, "web.config");
            File.WriteAllText(webConfigPath, webConfigContent);
            
            Debug.Log($"[WebGLBuildProcessor] web.config文件已创建: {webConfigPath}");
        }
        
        /// <summary>
        /// 创建Nginx配置文件
        /// </summary>
        private static void CreateNginxConfigFile(string buildPath)
        {
            string nginxConfigContent = @"# Unity WebGL的Nginx服务器配置建议
# 将此内容添加到您的nginx.conf文件中

location / {
    # 基本配置
    try_files $uri $uri/ /index.html;
    
    # 启用Gzip压缩
    gzip on;
    gzip_types
        text/plain
        text/css
        text/js
        text/xml
        text/javascript
        application/javascript
        application/xml+rss
        application/wasm
        application/octet-stream;
    
    # 设置MIME类型
    location ~* \.(wasm)$ {
        add_header Content-Type application/wasm;
        expires 1y;
        add_header Cache-Control ""public, immutable"";
    }
    
    location ~* \.(data|unityweb)$ {
        add_header Content-Type application/octet-stream;
        expires 1y;
        add_header Cache-Control ""public, immutable"";
    }
    
    # 启用CORS
    add_header Access-Control-Allow-Origin *;
    add_header Access-Control-Allow-Methods 'GET, POST, OPTIONS';
    add_header Access-Control-Allow-Headers '*';
}";
            
            string nginxConfigPath = Path.Combine(buildPath, "nginx-config-example.txt");
            File.WriteAllText(nginxConfigPath, nginxConfigContent);
            
            Debug.Log($"[WebGLBuildProcessor] Nginx配置示例已创建: {nginxConfigPath}");
        }
        
        /// <summary>
        /// 生成构建报告
        /// </summary>
        private static void GenerateBuildReport(BuildReport report, string buildPath)
        {
            Debug.Log("[WebGLBuildProcessor] 生成构建报告...");
            
            var reportData = new
            {
                buildTime = System.DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"),
                platform = report.summary.platform.ToString(),
                buildResult = report.summary.result.ToString(),
                totalTime = report.summary.totalTime.ToString(),
                buildSize = report.summary.totalSize,
                outputPath = report.summary.outputPath,
                playerSettings = new
                {
                    companyName = PlayerSettings.companyName,
                    productName = PlayerSettings.productName,
                    version = PlayerSettings.bundleVersion,
                    memorySize = PlayerSettings.WebGL.memorySize,
                    compressionFormat = PlayerSettings.WebGL.compressionFormat.ToString(),
                    // 🔥 新增移动端信息
                    screenSize = $"{PlayerSettings.defaultScreenWidth} x {PlayerSettings.defaultScreenHeight}",
                    nameFilesAsHashes = PlayerSettings.WebGL.nameFilesAsHashes
                },
                files = report.GetFiles()?.Length ?? 0,
                errors = report.summary.totalErrors,
                warnings = report.summary.totalWarnings
            };
            
            string reportJson = JsonUtility.ToJson(reportData, true);
            string reportPath = Path.Combine(buildPath, "build-report.json");
            File.WriteAllText(reportPath, reportJson);
            
            Debug.Log($"[WebGLBuildProcessor] 构建报告已生成: {reportPath}");
            Debug.Log($"[WebGLBuildProcessor] 构建结果: {report.summary.result}");
            Debug.Log($"[WebGLBuildProcessor] 构建时间: {report.summary.totalTime}");
            Debug.Log($"[WebGLBuildProcessor] 构建大小: {report.summary.totalSize} bytes");
        }
        
        /// <summary>
        /// 验证构建输出
        /// </summary>
        private static void ValidateBuildOutput(string buildPath)
        {
            Debug.Log("[WebGLBuildProcessor] 验证构建输出...");
            
            // 检查必要文件是否存在
            string[] requiredFiles = {
                "index.html",
                "Build"
            };
            
            bool allFilesExist = true;
            foreach (string file in requiredFiles)
            {
                string filePath = Path.Combine(buildPath, file);
                if (!File.Exists(filePath) && !Directory.Exists(filePath))
                {
                    Debug.LogError($"[WebGLBuildProcessor] 必要文件缺失: {filePath}");
                    allFilesExist = false;
                }
            }
            
            if (allFilesExist)
            {
                Debug.Log("[WebGLBuildProcessor] 构建输出验证通过");
            }
            else
            {
                Debug.LogError("[WebGLBuildProcessor] 构建输出验证失败");
            }
        }
        
        #endregion
        
        #region Menu Items
        
        /// <summary>
        /// 手动触发WebGL优化
        /// </summary>
        [MenuItem("SlotMachine/Build/Apply WebGL Optimizations")]
        public static void ManualApplyWebGLOptimizations()
        {
            Debug.Log("[WebGLBuildProcessor] 手动应用WebGL优化...");
            
            ApplyWebGLOptimizations();
            ConfigurePlayerSettings();
            OptimizeGraphicsSettings();
            ConfigureAudioSettings();
            ProcessStreamingAssets();
            ValidateWebGLTemplate();
            
            Debug.Log("[WebGLBuildProcessor] WebGL优化应用完成");
            EditorUtility.DisplayDialog("WebGL优化", "移动端WebGL优化设置已应用\n\n✅ 文件名哈希已禁用\n✅ 内存设置已优化\n✅ 移动端分辨率已设置", "确定");
        }
        
        /// <summary>
        /// 构建并优化WebGL
        /// </summary>
        [MenuItem("SlotMachine/Build/Build WebGL (Optimized)")]
        public static void BuildWebGLOptimized()
        {
            Debug.Log("[WebGLBuildProcessor] 开始优化WebGL构建...");
            
            // 应用优化
            ManualApplyWebGLOptimizations();
            
            // 获取构建路径
            string buildPath = EditorUtility.SaveFolderPanel("选择WebGL构建输出目录", "", "");
            if (string.IsNullOrEmpty(buildPath))
            {
                Debug.Log("[WebGLBuildProcessor] 构建已取消");
                return;
            }
            
            // 配置构建选项
            BuildPlayerOptions buildOptions = new BuildPlayerOptions
            {
                scenes = GetScenePaths(),
                locationPathName = buildPath,
                target = BuildTarget.WebGL,
                options = BuildOptions.None
            };
            
            // 开始构建
            BuildReport report = BuildPipeline.BuildPlayer(buildOptions);
            
            if (report.summary.result == BuildResult.Succeeded)
            {
                Debug.Log($"[WebGLBuildProcessor] WebGL构建成功: {buildPath}");
                EditorUtility.DisplayDialog("构建完成", $"移动端WebGL构建成功!\n\n路径: {buildPath}\n\n✅ iframe功能已自动注入\n✅ Safari兼容性已优化", "确定");
            }
            else
            {
                Debug.LogError($"[WebGLBuildProcessor] WebGL构建失败: {report.summary.result}");
                EditorUtility.DisplayDialog("构建失败", $"WebGL构建失败: {report.summary.result}", "确定");
            }
        }
        
        /// <summary>
        /// 获取场景路径
        /// </summary>
        private static string[] GetScenePaths()
        {
            var scenes = new List<string>();
            foreach (EditorBuildSettingsScene scene in EditorBuildSettings.scenes)
            {
                if (scene.enabled)
                {
                    scenes.Add(scene.path);
                }
            }
            return scenes.ToArray();
        }
        
        #endregion
    }
}