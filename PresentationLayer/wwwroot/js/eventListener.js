document.addEventListener('DOMContentLoaded', () => {
    const answers = document.querySelectorAll('.answer');
    let isAnswerSelected = false;

    answers.forEach(answer => {
        answer.addEventListener('click', () => {
            if (isAnswerSelected) {
                return;
            }

            isAnswerSelected = true;

            // Удаляем класс eventListener у всех элементов с классом .answer
            answers.forEach(a => {
                a.classList.remove('eventListener');
            });

            answer.classList.add('selected');

            // Выводим содержимое каждого выбранного ответа для отладки
            console.log(answer.textContent);

            const selectedAnswer = answer.textContent; // Получаем выбранный ответ
            console.log(selectedAnswer);

            // Отправляем выбранный ответ на сервер для проверки
            fetch(`/Game/CheckAnswer/${selectedAnswer}`)
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
