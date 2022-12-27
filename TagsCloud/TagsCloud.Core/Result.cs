﻿namespace TagsCloud.Core;

public record Result<T>(T Value, string? ErrorMessage = null)
{
	public bool IsFail => !IsSuccess;

	public bool IsSuccess => ErrorMessage is null;

	public static implicit operator T(Result<T> result)
	{
		if (result.IsFail)
			throw new ArgumentException("Trying cast Error result to value");

		return result.Value;
	}

	public static implicit operator Result<T>(T value) => new(value);
}

public class None
{
}

public static class Result
{
	public static Result<T> AsResult<T>(this T value)
	{
		return Ok(value);
	}

	public static Result<T> Ok<T>(T value)
	{
		return new Result<T>(value);
	}

	public static Result<None> Ok()
	{
		return Ok<None>(null);
	}

	public static Result<T> Fail<T>(string e)
	{
		return new Result<T>(default, e);
	}

	public static Result<T> Of<T>(Func<T> f, string? error = null)
	{
		try
		{
			return Ok(f());
		}
		catch (Exception e)
		{
			return Fail<T>(error ?? e.Message);
		}
	}

	public static Result<None> OfAction(Action f, string? error = null)
	{
		try
		{
			f();
			return Ok();
		}
		catch (Exception e)
		{
			return Fail<None>(error ?? e.Message);
		}
	}

	public static Result<TOutput> Then<TInput, TOutput>(
		this Result<TInput> input,
		Func<TInput, TOutput> continuation)
	{
		return input.Then(inp => Of(() => continuation(inp)));
	}

	public static Result<None> Then<TInput, TOutput>(
		this Result<TInput> input,
		Action<TInput> continuation)
	{
		return input.Then(inp => OfAction(() => continuation(inp)));
	}

	public static Result<None> Then<TInput>(
		this Result<TInput> input,
		Action<TInput> continuation)
	{
		return input.Then(inp => OfAction(() => continuation(inp)));
	}

	public static Result<TOutput> Then<TInput, TOutput>(
		this Result<TInput> input,
		Func<TInput, Result<TOutput>> continuation)
	{
		return input.IsSuccess
			? continuation(input.Value)
			: Fail<TOutput>(input.ErrorMessage);
	}

	public static Result<TInput> OnFail<TInput>(
		this Result<TInput> input,
		Action<string> handleError)
	{
		if (input.IsFail) handleError(input.ErrorMessage);
		return input;
	}

	public static Result<TInput> ReplaceError<TInput>(
		this Result<TInput> input,
		Func<string, string> replaceError)
	{
		if (input.IsSuccess) return input;
		return Fail<TInput>(replaceError(input.ErrorMessage));
	}

	public static Result<TInput> RefineError<TInput>(
		this Result<TInput> input,
		string errorMessage)
	{
		return input.ReplaceError(err => errorMessage + ". " + err);
	}
}