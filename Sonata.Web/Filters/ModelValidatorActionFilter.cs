#region Namespace Sonata.Web.Filters
//	TODO
#endregion

using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using System;
using System.Net;

namespace Sonata.Web.Filters
{
    public class ModelValidatorActionFilter : IActionFilter
    {
        #region Properties

        /// <summary>
        /// Gets or sets a value allowing to customize the <see cref="ModelValidatorActionFilter"/> behavior.
        /// </summary>
        public static ModelValidatorActionFilterOption Options { get; set; }

        #endregion

        #region Constructors

        /// <summary>
        /// Initializes a new instance of the <see cref="ModelValidatorActionFilterOption"/> class.
        /// </summary>
        public ModelValidatorActionFilter()
        {
            Options = new ModelValidatorActionFilterOption();
        }

        #endregion

        #region Methods

        #region IActionFilter Members

        public void OnActionExecuting(ActionExecutingContext filterContext)
        {
            if (!Options.IsValidationRunOnActionExecuting)
            {
                return;
            }

            Run(filterContext, null);
        }

        public void OnActionExecuted(ActionExecutedContext filterContext)
        {
            if (Options.IsValidationRunOnActionExecuting)
            {
                return;
            }

            Run(null, filterContext);
        }

        #endregion

        private void Run(ActionExecutingContext actionExecutingContext, ActionExecutedContext actionExecutedContext)
        {
            var isModelValid = Options.IsValidationRunOnActionExecuting
                ? actionExecutingContext.ModelState.IsValid
                : actionExecutedContext.ModelState.IsValid;

            if (!isModelValid)
            {
                var modelStateDictionary = Options.IsValidationRunOnActionExecuting
                    ? actionExecutingContext.ModelState
                    : actionExecutedContext.ModelState;
                var badRequestResult = new BadRequestObjectResult(modelStateDictionary)
                {
                    StatusCode = (int)Options.ResultingStatusCode
                };

                if (Options.IsValidationRunOnActionExecuting)
                {
                    actionExecutingContext.Result = badRequestResult;
                }
                else
                {
                    actionExecutedContext.Result = badRequestResult;
                }

                Options.OnModelStateInvalid?.Invoke(actionExecutingContext, actionExecutedContext);
            }
            else
            {
                Options.OnModelStateValid?.Invoke(actionExecutingContext, actionExecutedContext);
            }
        }

        #endregion
    }

    public class ModelValidatorActionFilterOption
    {
        #region Properties

        /// <summary>
        /// Gets or sets the <see cref="HttpStatusCode"/> returned by the <see cref="ModelValidatorActionFilter"/> when the current <see cref="ActionExecutingContext.ModelState"/> id invalid
        /// </summary>
        /// <remarks>The <see cref="ResultingStatusCode"/> default value is set to <see cref="HttpStatusCode.BadRequest"/>.</remarks>
        public HttpStatusCode ResultingStatusCode { get; set; }

        /// <summary>
        /// Gets or sets a value indicating if the validation process has to be run during the <see cref="IActionFilter.OnActionExecuting(ActionExecutingContext)"/> execution.
        ///  - if <see cref="IsValidationRunOnActionExecuting"/> is <c>true</c>, the validation process will be run only during <see cref="IActionFilter.OnActionExecuting(ActionExecutingContext)"/> execution
        ///  - if <see cref="IsValidationRunOnActionExecuting"/> is <c>false</c> the validation process will be run only during <see cref="IActionFilter.OnActionExecuted(ActionExecutedContext)"/> execution
        /// </summary>
        /// <remarks>The <see cref="IsValidationRunOnActionExecuting"/> default value is set to <c>true</c>.</remarks>
        public bool IsValidationRunOnActionExecuting { get; set; }

        /// <summary>
        /// Gets or sets a value indicating an <see cref="Action"/> to run once the valation process has finished only if the <see cref="ActionExecutingContext.ModelState"/> is invalid.
        /// </summary>
        /// <remarks>The <see cref="OnModelStateInvalid"/> default value is set to <c>null</c>.</remarks>
        public Action<ActionExecutingContext, ActionExecutedContext> OnModelStateInvalid { get; set; }

        /// <summary>
        /// Gets or sets a value indicating an <see cref="Action"/> to run once the valation process has finished only if the <see cref="ActionExecutedContext.ModelState"/> is invalid.
        /// </summary>
        /// <remarks>The <see cref="OnModelStateValid"/> default value is set to <c>null</c>.</remarks>
        public Action<ActionExecutingContext, ActionExecutedContext> OnModelStateValid { get; set; }

        #endregion

        #region Constructors

        /// <summary>
        /// Initializes a new instance of the <see cref="ModelValidatorActionFilterOption"/> class.
        /// </summary>
        /// <remarks>
        /// By default:
        ///     - The <see cref="ResultingStatusCode"/> default value is set to <see cref="HttpStatusCode.BadRequest"/>.
        ///     - The <see cref="IsValidationRunOnActionExecuting"/> default value is set to <c>true</c>.
        ///     - The <see cref="OnModelStateInvalid"/> default value is set to <c>null</c>.
        ///     - The <see cref="OnModelStateValid"/> default value is set to <c>null</c>.
        /// </remarks>
        public ModelValidatorActionFilterOption()
        {
            ResultingStatusCode = HttpStatusCode.BadRequest;
            IsValidationRunOnActionExecuting = true;
            OnModelStateInvalid = null;
            OnModelStateValid = null;
        }

        #endregion
    }
}
