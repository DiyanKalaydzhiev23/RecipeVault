let result = document.getElementById("result");
let searchBtn = document.getElementById("search-btn");

searchBtn.addEventListener("click", () => {
    let userInp = document.getElementById("user-inp").value;
    if (userInp.length === 0) {
        result.innerHTML = `<h3>Input Field Cannot Be Empty</h3>`;
        return;
    }

    fetch(`/recipe/api/${userInp}`)
        .then((response) => response.json())
        .then((data) => {
            if (!data.meals) {
                result.innerHTML = `<h3>No recipes found</h3>`;
                return;
            }

            let myMeal = data.meals[0];

            result.innerHTML = `
                <img src="${myMeal.image}" alt="Recipe Image">
                <div class="details">
                    <h2>${myMeal.name}</h2>
                </div>
                <div id="ingredient-con"></div>
                <div id="recipe" style="display:none;">
                    <button id="hide-recipe">X</button>
                    <pre id="instructions">${myMeal.instructions}</pre>
                </div>
                <div id="button-group" style="display: flex; gap: 10px; margin-top: 55px; margin-bottom: -20px; left: 0;"></div>
            `;

            let ingredientCon = document.getElementById("ingredient-con");
            let parent = document.createElement("ul");
            myMeal.ingredients.forEach((i) => {
                let child = document.createElement("li");
                child.innerText = i;
                parent.appendChild(child);
            });
            ingredientCon.appendChild(parent);

            document.getElementById("hide-recipe").addEventListener("click", () => {
                document.getElementById("recipe").style.display = "none";
            });

            const buttonGroup = document.getElementById("button-group");

            const viewBtn = document.createElement("button");
            viewBtn.id = "show-recipe";
            viewBtn.className = "btn btn-secondary";
            viewBtn.innerText = "View Recipe";
            buttonGroup.appendChild(viewBtn);

            document.getElementById("show-recipe").addEventListener("click", () => {
                document.getElementById("recipe").style.display = "block";
            });

            const recipeId = myMeal.id;

            if (myMeal.userId === currentUserId) {
                const editBtn = document.createElement("button");
                editBtn.id = "edit-recipe";
                editBtn.className = "btn btn-primary";
                editBtn.innerText = "Edit";

                const deleteBtn = document.createElement("button");
                deleteBtn.id = "delete-recipe";
                deleteBtn.className = "btn btn-danger";
                deleteBtn.innerText = "Delete";

                buttonGroup.appendChild(editBtn);
                buttonGroup.appendChild(deleteBtn);
                
                editBtn.addEventListener("click", () => {
                    window.location.href = `/Recipe/Edit/${recipeId}`;
                });
                
                deleteBtn.addEventListener("click", () => {
                    if (confirm("Are you sure you want to delete this recipe?")) {
                        const form = document.createElement("form");
                        form.method = "POST";
                        form.action = `/Recipe/Delete/${recipeId}`;
                        
                        const tokenInput = document.querySelector('input[name="__RequestVerificationToken"]');
                        if (tokenInput) {
                            const hiddenToken = document.createElement("input");
                            hiddenToken.type = "hidden";
                            hiddenToken.name = "__RequestVerificationToken";
                            hiddenToken.value = tokenInput.value;
                            form.appendChild(hiddenToken);
                        }

                        document.body.appendChild(form);
                        form.submit();
                    }
                });
            }
        })
        .catch(() => {
            result.innerHTML = `<h3>Invalid Input</h3>`;
        });
});
