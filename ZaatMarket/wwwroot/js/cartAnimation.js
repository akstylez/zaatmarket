// wwwroot/js/cartAnimation.js
window.animateToCart = (startElementId, targetElementId, imageUrl) => {
    const startEl = document.getElementById(startElementId);
    const targetEl = document.getElementById(targetElementId);

    if (!startEl || !targetEl) return;

    // Get the screen coordinates
    const startRect = startEl.getBoundingClientRect();
    const targetRect = targetEl.getBoundingClientRect();

    // Create a flying clone of the image
    const flyingImg = document.createElement('img');
    flyingImg.src = imageUrl;
    flyingImg.style.position = 'fixed';
    flyingImg.style.top = startRect.top + 'px';
    flyingImg.style.left = startRect.left + 'px';
    flyingImg.style.width = '50px'; // Make it a small thumbnail
    flyingImg.style.height = '50px';
    flyingImg.style.borderRadius = '50%';
    flyingImg.style.objectFit = 'cover';
    flyingImg.style.zIndex = '9999';
    flyingImg.style.transition = 'all 0.8s cubic-bezier(0.25, 1, 0.5, 1)';

    document.body.appendChild(flyingImg);

    
    void flyingImg.offsetWidth;


    flyingImg.style.top = targetRect.top + 'px';
    flyingImg.style.left = targetRect.left + 'px';
    flyingImg.style.opacity = '0.2'; 
    flyingImg.style.transform = 'scale(0.2)'; 

    
    setTimeout(() => {
        document.body.removeChild(flyingImg);
    }, 800);
};