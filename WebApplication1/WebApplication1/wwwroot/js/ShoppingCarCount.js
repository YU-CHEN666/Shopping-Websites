/*購物車圖示數量增加減少邏輯*/
const shoppingCarCount = document.getElementById('shoppingCarCount');
const tokenInput = document.querySelector('#shoppingCarForm input')
const formData = new FormData();
formData.append(tokenInput.name, tokenInput.value);
document.addEventListener('DOMContentLoaded', async (e) => {
    try {
        const response = await fetch('/api/ShoppingCarProcess/Count', {
            method: 'POST',
            body: formData,
        });
        if (!response.ok) throw new Error(`HTTP狀態碼:${response.status}`);
        const result = await response.text();
        if (result === '0') shoppingCarCount.style.display = 'none';
        else
        {
            shoppingCarCount.textContent = result;
            shoppingCarCount.style.display = 'block';
        }
    }
    catch (error) {
        console.log(error)
    }
});
function countIncrease() {
    const result = Number(shoppingCarCount.textContent) + 1;
    if (result > 99) {
        shoppingCarCount.textContent = '99+';
    }
    else {
        shoppingCarCount.textContent = result;
    }
    shoppingCarCount.style.display = 'block';
}
function countReduce() {
    const result = Number(shoppingCarCount.textContent) - 1;
    if (result === 0) {
        shoppingCarCount.textContent = '0';
        shoppingCarCount.style.display = 'none';
    }
    else {
        shoppingCarCount.textContent = result;
        shoppingCarCount.style.display = 'block';
    }
}