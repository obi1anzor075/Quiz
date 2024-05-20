document.addEventListener('DOMContentLoaded', () => {
    const answers = document.querySelectorAll('.answer');
    const nextQuestionBtn = document.getElementById('next-question-btn');
    let isAnswerSelected = false;

    nextQuestionBtn.setAttribute('disabled', 'true'); // Ensure the button is initially disabled

    function handleAnswerClick(event) {
        const answer = event.currentTarget;

        if (!isAnswerSelected) {
            isAnswerSelected = true;
            answer.classList.add('selected');

                nextQuestionBtn.removeAttribute('disabled'); // Enable the "Continue" button
                const selectedAnswer = answer.textContent.trim(); // Get the selected answer and trim whitespace

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
            
        }
    }

    answers.forEach(answer => {
        answer.addEventListener('click', handleAnswerClick);
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
