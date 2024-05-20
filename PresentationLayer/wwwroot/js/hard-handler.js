document.addEventListener('DOMContentLoaded', () => {
    const answers = document.querySelectorAll('.answer');
    const nextQuestionBtn = document.getElementById('next-question-btn');
    const inputAnswer = document.querySelector('.input-answer');

    nextQuestionBtn.setAttribute('disabled', 'true'); // Ensure the button is initially disabled

    function enableNextButton() {
        nextQuestionBtn.removeAttribute('disabled'); // Enable the "Continue" button
    }

    function handleAnswerClick(event) {
        const answer = event.currentTarget;
        answers.forEach(ans => ans.classList.remove('selected')); // Deselect other answers
        answer.classList.add('selected');
        enableNextButton();
    }

    answers.forEach(answer => {
        answer.addEventListener('click', handleAnswerClick);
    });

    if (inputAnswer) {
        inputAnswer.addEventListener('input', () => {
            if (inputAnswer.value.length > 1) {
                enableNextButton();
            } else {
                nextQuestionBtn.setAttribute('disabled', 'true'); // Disable the "Continue" button
            }
        });
    }

    // Reset counters when the back button is clicked
    const backButton = document.getElementById('backButton');
    if (backButton) {
        backButton.addEventListener('click', () => {
            fetch('/Game/ResetCounters')
                .catch(error => {
                    console.error('Error:', error);
                });
        });
    }
});
