/**
 * Unity WebGL JavaScript 插件
 * 实现 WebGLUtils.cs 中定义的所有 JavaScript 函数
 * 文件路径：Assets/Plugins/WebGLPlugin.jslib
 * 
 * 使用说明：
 * 1. 将此文件保存为 Assets/Plugins/WebGLPlugin.jslib
 * 2. 确保 Assets/Plugins/ 目录存在
 * 3. 重新构建 WebGL 项目
 */

mergeInto(LibraryManager.library, {

    // ================================================================================================
    // URL 和页面相关函数
    // ================================================================================================

    /**
     * 获取当前页面的完整URL
     */
    GetCurrentUrl: function () {
        var url = window.location.href;
        var bufferSize = lengthBytesUTF8(url) + 1;
        var buffer = _malloc(bufferSize);
        stringToUTF8(url, buffer, bufferSize);
        return buffer;
    },

    /**
     * 获取用户代理字符串
     */
    GetUserAgent: function () {
        var userAgent = navigator.userAgent || '';
        var bufferSize = lengthBytesUTF8(userAgent) + 1;
        var buffer = _malloc(bufferSize);
        stringToUTF8(userAgent, buffer, bufferSize);
        return buffer;
    },

    /**
     * 重新加载当前页面
     */
    ReloadPage: function () {
        try {
            window.location.reload();
        } catch (e) {
            console.error('ReloadPage failed:', e);
        }
    },

    /**
     * 打开新的URL
     */
    OpenUrl: function (urlPtr, targetPtr) {
        try {
            var url = UTF8ToString(urlPtr);
            var target = UTF8ToString(targetPtr) || '_blank';
            window.open(url, target);
        } catch (e) {
            console.error('OpenUrl failed:', e);
        }
    },

    // ================================================================================================
    // 屏幕和设备信息相关函数
    // ================================================================================================

    /**
     * 获取屏幕宽度
     */
    GetScreenWidth: function () {
        return screen.width || window.innerWidth || 1920;
    },

    /**
     * 获取屏幕高度
     */
    GetScreenHeight: function () {
        return screen.height || window.innerHeight || 1080;
    },

    /**
     * 获取设备像素比
     */
    GetDevicePixelRatio: function () {
        return window.devicePixelRatio || 1.0;
    },

    // ================================================================================================
    // 功能支持检测函数
    // ================================================================================================

    /**
     * 检查WebGL支持
     */
    CheckWebGLSupport: function () {
        try {
            var canvas = document.createElement('canvas');
            var gl = canvas.getContext('webgl') || canvas.getContext('experimental-webgl');
            return gl ? 1 : 0;
        } catch (e) {
            return 0;
        }
    },

    /**
     * 检查音频支持
     */
    CheckAudioSupport: function () {
        try {
            var AudioContext = window.AudioContext || window.webkitAudioContext;
            if (AudioContext) {
                var audioContext = new AudioContext();
                audioContext.close();
                return 1; // true
            }
            return 0; // false
        } catch (e) {
            return 0; // false
        }
    },

    /**
     * 检查本地存储支持
     */
    CheckLocalStorageSupport: function () {
        try {
            var test = '__localStorage_test__';
            localStorage.setItem(test, test);
            localStorage.removeItem(test);
            return 1; // true
        } catch (e) {
            return 0; // false
        }
    },

    // ================================================================================================
    // 页面通信相关函数
    // ================================================================================================

    /**
     * 向父窗口发送消息
     */
    PostMessageToParent: function (messagePtr) {
        try {
            var message = UTF8ToString(messagePtr);
            
            // 尝试向父窗口发送消息
            if (window.parent && window.parent !== window) {
                window.parent.postMessage(message, '*');
            }
            
            // 同时触发自定义事件（如果需要的话）
            var event = new CustomEvent('unityMessage', {
                detail: message
            });
            window.dispatchEvent(event);
            
        } catch (e) {
            console.error('PostMessageToParent failed:', e);
        }
    },

    // ================================================================================================
    // 本地存储相关函数
    // ================================================================================================

    /**
     * 获取本地存储项
     */
    GetLocalStorageItem: function (keyPtr) {
        try {
            var key = UTF8ToString(keyPtr);
            var value = localStorage.getItem(key) || '';
            var bufferSize = lengthBytesUTF8(value) + 1;
            var buffer = _malloc(bufferSize);
            stringToUTF8(value, buffer, bufferSize);
            return buffer;
        } catch (e) {
            console.error('GetLocalStorageItem failed:', e);
            var buffer = _malloc(1);
            stringToUTF8('', buffer, 1);
            return buffer;
        }
    },

    /**
     * 设置本地存储项
     */
    SetLocalStorageItem: function (keyPtr, valuePtr) {
        try {
            var key = UTF8ToString(keyPtr);
            var value = UTF8ToString(valuePtr);
            localStorage.setItem(key, value);
        } catch (e) {
            console.error('SetLocalStorageItem failed:', e);
        }
    },

    /**
     * 移除本地存储项
     */
    RemoveLocalStorageItem: function (keyPtr) {
        try {
            var key = UTF8ToString(keyPtr);
            localStorage.removeItem(key);
        } catch (e) {
            console.error('RemoveLocalStorageItem failed:', e);
        }
    },

    // ================================================================================================
    // 扩展功能函数（为了完整性添加）
    // ================================================================================================

    /**
     * 获取当前时间戳
     */
    GetCurrentTimestamp: function () {
        return Date.now();
    },

    /**
     * 检查移动设备
     */
    CheckMobileDevice: function () {
        var userAgent = navigator.userAgent || '';
        var mobileRegex = /Android|webOS|iPhone|iPad|iPod|BlackBerry|IEMobile|Opera Mini/i;
        return mobileRegex.test(userAgent) ? 1 : 0;
    },

    /**
     * 获取系统语言
     */
    GetSystemLanguage: function () {
        var language = navigator.language || navigator.userLanguage || 'en-US';
        var bufferSize = lengthBytesUTF8(language) + 1;
        var buffer = _malloc(bufferSize);
        stringToUTF8(language, buffer, bufferSize);
        return buffer;
    },

    /**
     * 复制文本到剪贴板
     */
    CopyToClipboard: function (textPtr) {
        try {
            var text = UTF8ToString(textPtr);
            if (navigator.clipboard && navigator.clipboard.writeText) {
                navigator.clipboard.writeText(text);
                return 1;
            } else {
                // 回退方法
                var textArea = document.createElement('textarea');
                textArea.value = text;
                textArea.style.position = 'fixed';
                textArea.style.opacity = '0';
                document.body.appendChild(textArea);
                textArea.select();
                var successful = document.execCommand('copy');
                document.body.removeChild(textArea);
                return successful ? 1 : 0;
            }
        } catch (e) {
            console.error('CopyToClipboard failed:', e);
            return 0;
        }
    },

    /**
     * 检查网络连接状态
     */
    CheckNetworkStatus: function () {
        return navigator.onLine ? 1 : 0;
    },

    /**
     * 振动设备（移动设备）
     */
    VibrateDevice: function (duration) {
        try {
            if ('vibrate' in navigator) {
                navigator.vibrate(duration);
                return 1;
            }
            return 0;
        } catch (e) {
            return 0;
        }
    },

    /**
     * 全屏API - 请求全屏
     */
    RequestFullscreen: function () {
        try {
            var element = document.documentElement;
            if (element.requestFullscreen) {
                element.requestFullscreen();
            } else if (element.mozRequestFullScreen) {
                element.mozRequestFullScreen();
            } else if (element.webkitRequestFullscreen) {
                element.webkitRequestFullscreen();
            } else if (element.msRequestFullscreen) {
                element.msRequestFullscreen();
            }
            return 1;
        } catch (e) {
            return 0;
        }
    },

    /**
     * 全屏API - 退出全屏
     */
    ExitFullscreen: function () {
        try {
            if (document.exitFullscreen) {
                document.exitFullscreen();
            } else if (document.mozCancelFullScreen) {
                document.mozCancelFullScreen();
            } else if (document.webkitExitFullscreen) {
                document.webkitExitFullscreen();
            } else if (document.msExitFullscreen) {
                document.msExitFullscreen();
            }
            return 1;
        } catch (e) {
            return 0;
        }
    },

    /**
     * 检查是否处于全屏状态
     */
    IsFullscreen: function () {
        return (document.fullscreenElement || 
                document.mozFullScreenElement || 
                document.webkitFullscreenElement || 
                document.msFullscreenElement) ? 1 : 0;
    },

    // ================================================================================================
    // Safari 特殊优化函数
    // ================================================================================================

    /**
     * Safari 音频上下文解锁（解决 Safari 音频策略限制）
     */
    UnlockSafariAudio: function () {
        try {
            var AudioContext = window.AudioContext || window.webkitAudioContext;
            if (AudioContext) {
                var context = new AudioContext();
                var buffer = context.createBuffer(1, 1, 22050);
                var source = context.createBufferSource();
                source.buffer = buffer;
                source.connect(context.destination);
                if (source.start) {
                    source.start(0);
                } else if (source.noteOn) {
                    source.noteOn(0);
                }
                return 1;
            }
            return 0;
        } catch (e) {
            console.error('UnlockSafariAudio failed:', e);
            return 0;
        }
    },

    /**
     * 检查是否为 Safari 浏览器
     */
    IsSafariBrowser: function () {
        var userAgent = navigator.userAgent || '';
        var isSafari = userAgent.indexOf('Safari') !== -1 && 
                      userAgent.indexOf('Chrome') === -1 && 
                      userAgent.indexOf('Edge') === -1;
        return isSafari ? 1 : 0;
    },

    /**
     * 获取可用内存信息（如果支持）
     */
    GetMemoryInfo: function () {
        try {
            if (performance && performance.memory) {
                return performance.memory.usedJSHeapSize / (1024 * 1024); // 返回 MB
            }
            return -1; // 不支持
        } catch (e) {
            return -1;
        }
    },

    // ================================================================================================
    // 调试和日志函数
    // ================================================================================================

    /**
     * 向浏览器控制台输出日志
     */
    ConsoleLog: function (messagePtr) {
        var message = UTF8ToString(messagePtr);
        console.log('[Unity] ' + message);
    },

    /**
     * 向浏览器控制台输出警告
     */
    ConsoleWarn: function (messagePtr) {
        var message = UTF8ToString(messagePtr);
        console.warn('[Unity] ' + message);
    },

    /**
     * 向浏览器控制台输出错误
     */
    ConsoleError: function (messagePtr) {
        var message = UTF8ToString(messagePtr);
        console.error('[Unity] ' + message);
    }

});