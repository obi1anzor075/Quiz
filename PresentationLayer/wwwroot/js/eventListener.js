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

            const selectedAnswer = answer.textContent.trim(); // Получаем выбранный ответ, удаляем пробелы в начале и в конце

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
