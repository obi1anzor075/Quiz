document.addEventListener('DOMContentLoaded', () => {
    const nextQuestionBtn = document.getElementById('next-question-btn');
    const inputAnswer = document.querySelector('.input-answer');
    const confirmBtn = document.querySelector('.confirm-btn');

    // Ensure the "Continue" button is initially disabled
    nextQuestionBtn.setAttribute('disabled', 'true');

    function enableNextButton() {
        nextQuestionBtn.removeAttribute('disabled'); // Enable the "Continue" button
    }

    // Handle the "Confirm" button click event
    confirmBtn.addEventListener('click', async (event) => {
        event.preventDefault(); // Prevent default link behavior

        let selectedAnswer = inputAnswer.value.trim();

        // Ensure an answer is entered
        if (selectedAnswer) {
            confirmBtn.classList.add('disabled'); // Disable the confirm button to prevent multiple clicks

            try {
                const response = await fetch(`/Game/CheckHardAnswer/${selectedAnswer}`);
                const data = await response.json();

                    if (data.isCorrect) {
                        inputAnswer.classList.add('correct');
                    } else {
                        inputAnswer.classList.add('incorrect');
                    }

                    enableNextButton();
            } catch (error) {
                console.error('Error:', error);
            }
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
