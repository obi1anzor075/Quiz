document.addEventListener('DOMContentLoaded', () => {
    const nextQuestionBtn = document.getElementById('next-question-btn');
    const inputAnswer = document.querySelector('.input-answer');
    const confirmBtn = document.querySelector('.confirm-btn');
    let isChecking = false; // ���� ��� �������� ��������� ��������

    // Ensure the "Continue" button is initially disabled
    nextQuestionBtn.setAttribute('disabled', 'true');

    function enableNextButton() {
        nextQuestionBtn.removeAttribute('disabled'); // Enable the "Continue" button
    }

    if (inputAnswer && confirmBtn) {
        inputAnswer.addEventListener('keypress', (event) => {
            if (event.key === 'Enter') {
                event.preventDefault(); // ������������� �������� �����
                if (!confirmBtn.classList.contains('disabled') && !isChecking) {
                    confirmBtn.click();
                }
            }
        });
    }

    // Handle the "Confirm" button click event
    confirmBtn.addEventListener('click', async (event) => {
        event.preventDefault(); // Prevent default link behavior

        if (isChecking) {
            return; // ��������� ��������, ���� ��� ��� ����
        }

        let selectedAnswer = inputAnswer.value.trim();

        // Ensure an answer is entered
        if (selectedAnswer) {
            confirmBtn.classList.add('disabled'); // Disable the confirm button to prevent multiple clicks
            isChecking = true; // ���������� ���� ��������

            try {
                const response = await fetch(`/Game/CheckHardAnswer/${selectedAnswer}`);
                const data = await response.json();


                    if (data.isCorrect) {
                        inputAnswer.classList.add('correct');
                    } else {
                        inputAnswer.classList.add('incorrect');
                    }

                    enableNextButton();
                    isChecking = false; // �������� ���� �������� ����� ����������

            } catch (error) {
                console.error('Error:', error);
                isChecking = false; // �������� ���� �������� � ������ ������
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

