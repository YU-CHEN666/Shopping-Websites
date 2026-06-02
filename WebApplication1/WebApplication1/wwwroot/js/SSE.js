//專門接收SSE，並做出相對應處置
const layoutUserName = document.getElementById("layoutUserName");
const eventSource = new EventSource('/api/SSE/CleanupNotice');
eventSource.onmessage = function (event) {
    if (!layoutUserName)
    {
        if (Number(shoppingCarCount.textContent) > 0) {
            MessageDisplayInformation('購物車已被清除，請重新整理頁面');
        }
    }
};