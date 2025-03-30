//Initial References
let result = document.getElementById("result");
let searchBtn = document.getElementById("search-btn");

searchBtn.addEventListener("click", () => {
    let userInp = document.getElementById("user-inp").value;
    if (userInp.length == 0) {
        result.innerHTML = `<h3>Input Field Cannot Be Empty</h3>`;
    } else {
        fetch(`/recipe/api/${userInp}`)
            .then((response) => response.json())
            .then((data) => {
                if (!data.meals) {
                    result.innerHTML = `<h3>No recipes found</h3>`;
                    return;
                }

                let myMeal = data.meals[0];

                result.innerHTML = `
                  <img src=${myMeal.image}>
                  <div class="details">
                      <h2>${myMeal.name}</h2>
                  </div>
                  <div id="ingredient-con"></div>
                  <div id="recipe">
                      <button id="hide-recipe">X</button>
                      <pre id="instructions">${myMeal.instructions}</pre>
                  </div>
                  <button id="show-recipe">View Recipe</button>
                `;

                let ingredientCon = document.getElementById("ingredient-con");
                let parent = document.createElement("ul");
                myMeal.ingredients.forEach((i) => {
                    let child = document.createElement("li");
                    child.innerText = i;
                    parent.appendChild(child);
                    ingredientCon.appendChild(parent);
                });

                document.getElementById("hide-recipe").addEventListener("click", () => {
                    document.getElementById("recipe").style.display = "none";
                });

                document.getElementById("show-recipe").addEventListener("click", () => {
                    document.getElementById("recipe").style.display = "block";
                });
            })
            .catch(() => {
                result.innerHTML = `<h3>Invalid Input</h3>`;
            });
    }
});
