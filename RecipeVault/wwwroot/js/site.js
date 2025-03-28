﻿//Initial References
let result = document.getElementById("result");
let searchBtn = document.getElementById("search-btn");

searchBtn.addEventListener("click", () => {
    let userInp = document.getElementById("user-inp").value;
    if (userInp.length == 0) {
        result.innerHTML = `<h3>Input Field Cannot Be Empty</h3>`;
    } else {
        fetch(`/api/recipes/${userInp}`)
            .then((response) => response.json())
            .then((data) => {
                if (!data.meals) {
                    result.innerHTML = `<h3>No recipes found</h3>`;
                    return;
                }

                let myMeal = data.meals[0];
                let count = 1;
                let ingredients = [];
                for (let i in myMeal) {
                    if (i.startsWith("strIngredient") && myMeal[i]) {
                        let ingredient = myMeal[i];
                        let measure = myMeal[`strMeasure` + count];
                        count += 1;
                        ingredients.push(`${measure} ${ingredient}`);
                    }
                }

                result.innerHTML = `
          <img src=${myMeal.strMealThumb}>
          <div class="details">
              <h2>${myMeal.strMeal}</h2>
              <h4>${myMeal.strArea}</h4>
          </div>
          <div id="ingredient-con"></div>
          <div id="recipe">
              <button id="hide-recipe">X</button>
              <pre id="instructions">${myMeal.strInstructions}</pre>
          </div>
          <button id="show-recipe">View Recipe</button>
        `;

                let ingredientCon = document.getElementById("ingredient-con");
                let parent = document.createElement("ul");
                ingredients.forEach((i) => {
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
