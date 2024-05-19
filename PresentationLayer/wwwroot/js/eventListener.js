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
                a.removeEventListener('click', () => { }); // Удаляем обработчик события клика
            });

            answer.classList.add('selected');

            // Добавляем задержку в 3 секунды перед выполнением логики
            setTimeout(() => {
                const selectedAnswer = answer.textContent.trim(); // Получаем выбранный ответ, удаляем пробелы в начале и в конце

                fetch(`/Game/CheckAnswer/${selectedAnswer}`)
                    .then(response => response.json())
                    .then(data => {
                        if (data.isCorrect) {
                            answer.classList.add('correct');
                        } else {
                            answer.classList.add('incorrect');
                        }

                        // Включаем кнопку "Продолжить" после проверки ответа
                        document.getElementById('next-question-btn').removeAttribute('disabled');
                    })
                    .catch(error => {
                        console.error('Error:', error);
                    });
            }, 3000); // Задержка в 3 секунды
        });
    });
});
