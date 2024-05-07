document.addEventListener('DOMContentLoaded', () => {
    const answers = document.querySelectorAll('.answer');
    let isAnswerSelected = false;

    answers.forEach(answer => {
        answer.addEventListener('click', () => {
            if (isAnswerSelected) {
                return;
            }

            isAnswerSelected = true;

            answer.classList.add('selected');

            const selectedAnswer = answer.textContent.trim(); // Получаем выбранный ответ, удаляем пробелы в начале и в конце

            // Получаем закодированный правильный ответ из скрытого поля
            const encodedCorrectAnswer = document.getElementById('correctAnswer').value;

            // Раскодируем закодированный правильный ответ
            const correctAnswer = document.getElementById('encodedCorrectAnswer').value;

            // Сравниваем выбранный ответ с раскодированным правильным ответом
            if (selectedAnswer === correctAnswer) {
                answer.classList.add('correct');
            } else {
                answer.classList.add('incorrect');
            }
        });
    });
});
