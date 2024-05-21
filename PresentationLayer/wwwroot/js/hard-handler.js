document.addEventListener('DOMContentLoaded', () => {
    const answers = document.querySelectorAll('.answer');
    const nextQuestionBtn = document.getElementById('next-question-btn');
    const inputAnswer = document.querySelector('.input-answer');

    nextQuestionBtn.setAttribute('disabled', 'true'); // Ensure the button is initially disabled

    function enableNextButton() {
        nextQuestionBtn.removeAttribute('disabled'); // Enable the "Continue" button
    }

    // Handle the "Continue" button click event
    nextQuestionBtn.addEventListener('click', () => {
        let selectedAnswer = null;

        // Check if any answer is selected
        answers.forEach(answer => {
            if (answer.classList.contains('selected')) {
                selectedAnswer = answer.textContent.trim();
            }
        });

        // If there's an input answer, use it instead
        if (inputAnswer && inputAnswer.value.trim().length > 0) {
            selectedAnswer = inputAnswer.value.trim();
        }

        // Ensure an answer is selected
        if (selectedAnswer) {
            fetch(`/Game/CheckAnswer/${selectedAnswer}`)
                .then(response => response.json())
                .then(data => {
                    if (data.isCorrect) {
                        inputAnswer.classList.add('correct');
                        enableNextButton();
                    } else {
                        inputAnswer.classList.add('incorrect');
                        enableNextButton();
                    }
                })

                .catch(error => {
                    console.error('Error:', error);
                });
        }
    });

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
