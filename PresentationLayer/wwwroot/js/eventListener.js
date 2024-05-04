document.addEventListener('DOMContentLoaded', () => {
    const answers = document.querySelectorAll('.answer');
    let isAnswerSelected = false;

    answers.forEach(answer => {
        answer.addEventListener('click', () => {
            if (isAnswerSelected) {
                return;
            }

            isAnswerSelected = true;

            answers.forEach(a => {
                a.classList.remove('selected');
                a.classList.remove('eventListener');
            });

            answer.classList.add('selected');

            const selectedAnswer = answer.textContent; // Получаем выбранный ответ
            // Отправляем выбранный ответ на сервер для проверки
            fetch('/Game/CheckAnswer', {
                method: 'POST',
                headers: {
                    'Content-Type': 'application/json'
                },
                body: JSON.stringify({ selectedAnswer: selectedAnswer })
            })
                .then(response => response.json())
                .then(data => {
                    if (data.isCorrect) {
                        answer.classList.add('correct');
                    } else {
                        answer.classList.add('incorrect');
                    }
                })
                .catch(error => {
                    console.error('Error:', error);
                });
        });
    });
});
