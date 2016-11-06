SimpleWebSocket
=====
[![xpy MyGet Build Status](https://www.myget.org/BuildSource/Badge/xpy?identifier=58572f41-bfc9-4394-b976-e7cc1275e6be)](https://www.myget.org/)

本套件簡化ASP.net Core原有之WebSocket使用方式，透過本套件提供的Handler來簡易處理WebSocket操作

-----
### 安裝
1. [MyGet](https://www.myget.org/feed/xpy/package/nuget/SimpleWebSocket)
```
PM> Install-Package SimpleWebSocket
```

### 快速上手
#### 1.建構WebSocketHandler處理類別
```csharp
//使用本套件的所有WebsocketHandler都必須繼承WebSocketHandler抽象類別
public class WsEchoHandler : WebSocketHandler {
    //必須為無參數建構子
    public WsEchoHandler() : 
        base("/api/echo") {//呼叫基底類別建構子用以設定路由

        //設定接收訊息時的事件，除此事件外還有許多事件(見WebSocketHandler類)
        this.OnReceived += WsEchoHandler_OnReceived;
    }

    //當WebSocket接收到訊息的時候觸發的事件
    private async void WsEchoHandler_OnReceived(System.Net.WebSockets.WebSocket socket, System.Net.WebSockets.WebSocketMessageType type, byte[] receiveMessage) {
        //echo
        await socket.SendAsync(receiveMessage, type,true, this.BufferSize);
    }

    //必須實作的方法，此方法用以實作由HttpContext判斷是否允許連線
    protected override bool AcceptConditions(HttpContext Context) {
        return true;
    }
}
```

#### 2.於ASP.net Core啟動類別設定，即可使用
```csharp
using SimpleWebSocket;
...(something)...
public class Startup {
    ...(something)...
     public void Configure(IApplicationBuilder app, IHostingEnvironment env, ILoggerFactory loggerFactory) {
         ...(something)...
         app.UseWebSockets<WsEchoHandler>();
     }
}
```

### WebSocketHandler類別事件列表(依運行管線排列)
```
WebSocket請求，AcceptConditions方法確認是否允許
├─OnAcceptConnected: 當符合連線條件時觸發
│ ├─OnConnected: 當WebSocket開啟連線時
│ ├─OnReceiving: 當WebSocket正在接收訊息片段
│ ├─OnReceived: 當WebSocket接收到訊息
│ ├─OnException: 當WebSocket服務發生例外
│ └─OnDisconnected: 當WebSocket關閉連線時
└─OnDenyConnected: 當不符合連線條件時觸發
```

### 許可證
本專案使用MIT許可證