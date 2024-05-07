document.addEventListener('DOMContentLoaded', () => {
    const nextQuestionBtn = document.getElementById('nextQuestionBtn');

    nextQuestionBtn.addEventListener('click', () => {
        fetch(`/SelectMode/Easy?index=${currentQuestionIndex + 1}`)
            .then(response => response.text())
            .then(data => {
                window.location.reload(); // Перезагрузить страницу для отображения следующего вопроса
            })
            .catch(error => {
                console.error('Error:', error);
            });
    });
});
