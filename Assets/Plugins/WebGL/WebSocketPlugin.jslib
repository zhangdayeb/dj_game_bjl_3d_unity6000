// Assets/Plugins/WebGL/WebSocketPlugin.jslib
// WebSocket JavaScript插件，为Unity WebGL提供WebSocket功能

var WebSocketPlugin = {
    $WebSocketState: {
        instances: {},
        nextId: 1,
        
        // WebSocket状态枚举
        CONNECTING: 0,
        OPEN: 1,
        CLOSING: 2,
        CLOSED: 3
    },
    
    InitWebSocket: function(url) {
        var urlStr = UTF8ToString(url);
        console.log('[WebSocketPlugin] 初始化WebSocket连接:', urlStr);
        
        try {
            // 创建WebSocket实例
            var ws = new WebSocket(urlStr);
            var instanceId = WebSocketState.nextId++;
            WebSocketState.instances[instanceId] = ws;
            
            // 设置事件处理器
            ws.onopen = function(event) {
                console.log('[WebSocketPlugin] WebSocket连接已打开');
                // 调用Unity的C#回调方法
                SendMessage('WebSocketManager', 'OnConnected', '');
            };
            
            ws.onmessage = function(event) {
                console.log('[WebSocketPlugin] 收到WebSocket消息:', event.data);
                // 调用Unity的C#回调方法
                SendMessage('WebSocketManager', 'OnMessageReceived', event.data);
            };
            
            ws.onerror = function(event) {
                console.error('[WebSocketPlugin] WebSocket错误:', event);
                // 调用Unity的C#回调方法
                SendMessage('WebSocketManager', 'OnError', 'WebSocket连接错误');
            };
            
            ws.onclose = function(event) {
                console.log('[WebSocketPlugin] WebSocket连接已关闭, 代码:', event.code, '原因:', event.reason);
                // 调用Unity的C#回调方法
                SendMessage('WebSocketManager', 'OnDisconnected', event.reason || 'Unknown');
                
                // 清理实例
                delete WebSocketState.instances[instanceId];
            };
            
            // 存储当前活动的实例ID
            WebSocketState.currentInstanceId = instanceId;
            
        } catch (error) {
            console.error('[WebSocketPlugin] 创建WebSocket失败:', error);
            SendMessage('WebSocketManager', 'OnError', error.message || 'Failed to create WebSocket');
        }
    },
    
    SendWebSocketMessage: function(message) {
        try {
            var messageStr = UTF8ToString(message);
            var instanceId = WebSocketState.currentInstanceId;
            
            if (instanceId && WebSocketState.instances[instanceId]) {
                var ws = WebSocketState.instances[instanceId];
                
                if (ws.readyState === WebSocket.OPEN) {
                    console.log('[WebSocketPlugin] 发送WebSocket消息:', messageStr);
                    ws.send(messageStr);
                } else {
                    console.warn('[WebSocketPlugin] WebSocket未处于OPEN状态，无法发送消息. 当前状态:', ws.readyState);
                }
            } else {
                console.error('[WebSocketPlugin] 没有找到有效的WebSocket实例');
            }
        } catch (error) {
            console.error('[WebSocketPlugin] 发送WebSocket消息失败:', error);
        }
    },
    
    CloseWebSocket: function() {
        try {
            var instanceId = WebSocketState.currentInstanceId;
            
            if (instanceId && WebSocketState.instances[instanceId]) {
                var ws = WebSocketState.instances[instanceId];
                console.log('[WebSocketPlugin] 关闭WebSocket连接');
                
                if (ws.readyState === WebSocket.OPEN || ws.readyState === WebSocket.CONNECTING) {
                    ws.close(1000, 'Normal closure');
                }
                
                delete WebSocketState.instances[instanceId];
                delete WebSocketState.currentInstanceId;
            }
        } catch (error) {
            console.error('[WebSocketPlugin] 关闭WebSocket失败:', error);
        }
    },
    
    GetWebSocketState: function() {
        try {
            var instanceId = WebSocketState.currentInstanceId;
            
            if (instanceId && WebSocketState.instances[instanceId]) {
                var ws = WebSocketState.instances[instanceId];
                return ws.readyState;
            }
            
            return WebSocketState.CLOSED;
        } catch (error) {
            console.error('[WebSocketPlugin] 获取WebSocket状态失败:', error);
            return WebSocketState.CLOSED;
        }
    }
};

// 注册插件方法
autoAddDeps(WebSocketPlugin, '$WebSocketState');
mergeInto(LibraryManager.library, WebSocketPlugin);