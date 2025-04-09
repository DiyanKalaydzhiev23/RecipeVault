// Grabbing references to DOM elements
let result = document.getElementById("result");
let searchBtn = document.getElementById("search-btn");

// Ensure currentUserId is set from server-side (e.g. in a script tag in your Layout)
const currentUserId = '@User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value';

function addIngredient() {
    const wrapper = document.getElementById('ingredients-wrapper');
    const input = document.createElement('input');
    input.type = 'text';
    input.name = 'IngredientNames';
    input.className = 'form-control mb-2';
    input.placeholder = 'e.g. Basil';

    wrapper.appendChild(input);
}

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

            // Build the main structure for the recipe display
            result.innerHTML = `
                <img src="${myMeal.image}" alt="Recipe Image">
                <div class="details">
                    <h2>${myMeal.name}</h2>
                </div>
                <div id="ingredient-con"></div>

                <!-- Recipe instructions hidden by default -->
                <div id="recipe" style="display:none;">
                    <button id="hide-recipe">X</button>
                    <pre id="instructions">${myMeal.instructions}</pre>
                </div>

                <!-- Buttons for viewing, editing, deleting, etc. -->
                <div id="button-group" style="display: flex; gap: 10px; margin-top: 55px; margin-bottom: -20px;">
                </div>
            `;

            // ----- INGREDIENTS -----
            let ingredientCon = document.getElementById("ingredient-con");
            let parentUl = document.createElement("ul");

            // Render existing ingredients
            myMeal.ingredients.forEach((i) => {
                let li = document.createElement("li");
                li.innerText = i;
                parentUl.appendChild(li);
            });

            ingredientCon.appendChild(parentUl);

            // Create an "Add Ingredient" button (if you want always visible or only for recipe owners)
            // If you only want owners to have this, wrap it in: if (myMeal.userId === currentUserId) { ... }
            const addIngredientBtn = document.createElement("button");
            addIngredientBtn.id = "add-ingredient";
            addIngredientBtn.innerText = "+ Add Ingredient";
            addIngredientBtn.className = "btn btn-success";

            // Click handler: add a new <li><input/></li>
            addIngredientBtn.addEventListener("click", () => {
                const newLi = document.createElement("li");
                const input = document.createElement("input");
                input.type = "text";
                input.placeholder = "New Ingredient";
                newLi.appendChild(input);
                parentUl.appendChild(newLi);
            });

            // Append the button to the ingredients container
            ingredientCon.appendChild(addIngredientBtn);

            // ----- Hide/Show Instructions -----
            document.getElementById("hide-recipe").addEventListener("click", () => {
                document.getElementById("recipe").style.display = "none";
            });

            const viewBtn = document.createElement("button");
            viewBtn.id = "show-recipe";
            viewBtn.className = "btn btn-secondary";
            viewBtn.innerText = "View Recipe";

            const buttonGroup = document.getElementById("button-group");
            buttonGroup.appendChild(viewBtn);

            viewBtn.addEventListener("click", () => {
                document.getElementById("recipe").style.display = "block";
            });

            // ----- Edit/Delete Buttons for Owners -----
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

                        // If you have the anti-forgery token in the DOM
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
